# AWS Lambda Setup

This guide details how to deploy `AttachmentCollector` as an AWS Lambda function using either the AWS Console or the AWS CLI.

## Option 1: AWS Console (GUI)

### 1. Build and Package
Navigate to the project directory and create a deployment package.

```bash
cd AttachmentCollector.ConsoleApp
dotnet publish -c Release -r linux-x64 --self-contained false -o publish
cd publish
zip -r ../deployment-package.zip *
```

### 2. Create the Lambda Function
1. Go to the **AWS Lambda Console**.
2. Click **Create function**.
3. Select **Author from scratch**.
4. Settings:
   - **Runtime**: .NET 8
   - **Architecture**: x86_64
5. Click **Create function**.

### 3. Upload Code
1. In the **Code** tab, click **Upload from** -> **.zip file**.
2. Select the `deployment-package.zip` created in Step 1.

### 4. Configure Runtime Settings
Edit the **Runtime settings** to point to the correct handler:
- **Handler**: `AttachmentCollector.ConsoleApp::AttachmentCollector.ConsoleApp.Function::FunctionHandler`

### 5. Configure Environment Variables
1. Go to the **Configuration** tab.
2. Select **Environment variables** -> **Edit**.
3. Add the following key-value pairs:

| Key | Value |
|-----|-------|
| `GOOGLE_CLIENT_ID` | Your Google Cloud Client ID |
| `GOOGLE_CLIENT_SECRET` | Your Google Cloud Client Secret |
| `GOOGLE_REFRESH_TOKEN` | Your valid Refresh Token |

### 6. Adjust Timeout
1. Go to **Configuration** -> **General configuration** -> **Edit**.
2. Increase **Timeout** (e.g., to 60 seconds).

---

## Option 2: AWS CLI

### 1. Prerequisites
Install the AWS CLI and Lambda tools.

```bash
# Install Lambda .NET tools
dotnet tool install -g Amazon.Lambda.Tools

# Configure AWS Credentials
aws configure
```

### 2. Build & Package

```bash
cd AttachmentCollector.ConsoleApp
dotnet lambda package -o deployment.zip
```

### 3. Create IAM Role
Create a role with basic execution permissions.

```bash
# Create trust policy file
cat > trust-policy.json << 'EOF'
{
  "Version":  "2012-10-17",
  "Statement": [{
    "Effect":  "Allow",
    "Principal": { "Service": "lambda.amazonaws.com" },
    "Action": "sts:AssumeRole"
  }]
}
EOF

# Create role
aws iam create-role \
  --role-name AttachmentCollectorRole \
  --assume-role-policy-document file://trust-policy.json

# Attach basic execution policy
aws iam attach-role-policy \
  --role-name AttachmentCollectorRole \
  --policy-arn arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole
```

### 4. Deploy Function
Create the function and upload the code.

```bash
# Get your account ID
ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)

# Create the function
aws lambda create-function \
  --function-name AttachmentCollector \
  --runtime dotnet8 \
  --role arn:aws:iam::${ACCOUNT_ID}:role/AttachmentCollectorRole \
  --handler AttachmentCollector.ConsoleApp::AttachmentCollector.ConsoleApp.Function::FunctionHandler \
  --zip-file fileb://deployment.zip \
  --timeout 60 \
  --memory-size 256
```

### 5. Configure Environment Variables

```bash
aws lambda update-function-configuration \
  --function-name AttachmentCollector \
  --environment "Variables={GOOGLE_CLIENT_ID=your-client-id,GOOGLE_CLIENT_SECRET=your-secret,GOOGLE_REFRESH_TOKEN=your-token}"
```

### 6. Test Manually

```bash
aws lambda invoke \
  --function-name AttachmentCollector \
  --payload '{}' \
  response.json

cat response.json
```

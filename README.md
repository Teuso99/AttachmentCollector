# AttachmentCollector

## About
AttachmentCollector is a console app that saves attachments from Gmail in Google Drive. 

## How to Use
Open the console and navigate to the main folder of the project. Then use command:

```bash
dotnet restore
```
Then, after restoring all the dependencies, build the project:

```bash
dotnet build
```

Finally, run the project passing the path to the secret file

```bash
AttachmentCollector.ConsoleApp.exe secret_file.json
```

## License
This project is for study purposes only. Feel free to use.

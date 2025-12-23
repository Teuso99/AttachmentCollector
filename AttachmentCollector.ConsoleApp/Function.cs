using Amazon.Lambda.Core;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AttachmentCollector.ConsoleApp;

public class Function
{
    public async Task FunctionHandler(ILambdaContext context)
    {
        await Program.RunAsync();
    }
}
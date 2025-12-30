using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AttachmentCollector.ConsoleApp;

public class Function
{
    private readonly ServiceProvider _serviceProvider = Startup.ConfigureServices();

    public async Task FunctionHandler(ILambdaContext context)
    {
        var runner = _serviceProvider.GetRequiredService<Runner>();
        await runner.RunAsync();
    }
}
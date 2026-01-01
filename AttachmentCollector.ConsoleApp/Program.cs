using AttachmentCollector.ConsoleApp;
using Microsoft.Extensions.DependencyInjection;

using var serviceProvider = Startup.ConfigureServices();
var runner = serviceProvider.GetRequiredService<Runner>();
await runner.RunAsync();
using AttachmentCollector.ConsoleApp.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Gmail.v1;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AttachmentCollector.ConsoleApp;

public static class Startup
{
    public static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Logging
        services.AddLogging(builder =>
        {
            builder
                .SetMinimumLevel(LogLevel.Information)
                .AddJsonConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.TimestampFormat = "yyyy-MM-dd HH:mm: ss ";
                });
        });

        // Google Credential
        services.AddSingleton<UserCredential>(_ =>
        {
            const string userId = "me";
            string[] scopes = [DriveService.Scope.Drive, GmailService.Scope.GmailModify];

            return new UserCredential(
                new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID"),
                        ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET")
                    },
                    Scopes = scopes
                }),
                userId,
                new TokenResponse { RefreshToken = Environment.GetEnvironmentVariable("GOOGLE_REFRESH_TOKEN") }
            );
        });

        // Services
        services.AddSingleton<IGmailClientService>(sp =>
        {
            var credential = sp.GetRequiredService<UserCredential>();
            var logger = sp.GetRequiredService<ILogger<GmailClientService>>();
            return new GmailClientService(credential, "me", logger);
        });

        services.AddSingleton<IDriveClientService>(sp =>
        {
            var credential = sp.GetRequiredService<UserCredential>();
            var logger = sp.GetRequiredService<ILogger<DriveClientService>>();
            return new DriveClientService(credential, logger);
        });

        // Main runner
        services.AddTransient<Runner>();

        return services.BuildServiceProvider();
    }
}
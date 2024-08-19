// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Serilog;
using SubRedditStats;
using SubRedditStats.Models;
using SubRedditStats.Services;
using System.Net.Http.Headers;
using System.Threading.Tasks;

// Set up Serilog for logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File("logs/redditclient.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

// Create the HostBuilder
var host = CreateHostBuilder(args).Build();

// Run the application
var logger = host.Services.GetRequiredService<ILogger<Program>>();
var redditClient = host.Services.GetRequiredService<ISubRedditService>();
var cancellationTokenSource = new CancellationTokenSource();

try
{
    logger.LogInformation("Starting Reddit Client...");

    var httpClient = host.Services.GetRequiredService<HttpClient>();
    httpClient.DefaultRequestHeaders.Add("User-Agent", "RedditClient/2.0");


    await redditClient.RunAsync(Environment.ProcessorCount, cancellationTokenSource.Token); // Running with 1 concurrent thread
}
catch (Exception ex)
{
    logger.LogError(ex, "An unhandled exception occurred.");
}
finally
{
    cancellationTokenSource.Cancel();
    logger.LogInformation("Reddit Client stopped.");
    Log.CloseAndFlush();
}

static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                // Register services and dependencies
                services.Configure<AppSettings>(context.Configuration.GetSection("AppSettings"));
                services.AddSingleton<HttpClient>();
                services.AddTransient<ITokenService, TokenService>();
                services.AddTransient<ISubRedditService, SubRedditService>();
                services.AddSingleton<ITokenRequestBuilder, TokenRequestBuilder>();
                services.AddSingleton<IRateLimitingService, RateLimitingService>();
                services.AddSingleton(new SemaphoreSlim(1, 1));

                // Add logging
                services.AddLogging(configure => configure.AddSerilog());
            });
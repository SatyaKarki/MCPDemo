using MCP.Server.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace MCP.Server;

/// <summary>
/// MCP Server application that exposes various tools for weather, calculator, todo management, and text utilities.
/// </summary>
public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Configure logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Information);

        // Configure HttpClient for API calls
        builder.Services.AddHttpClient();

        // Configure MCP Server with stdio transport
        builder.Services
            .AddMcpServer()
            .WithStdioServerTransport()
            .WithToolsFromAssembly(typeof(WeatherTool).Assembly)
            .WithToolsFromAssembly(typeof(ProductTool).Assembly);

        var app = builder.Build();

        await app.RunAsync();
    }

    /// <summary>
    /// Creates a configured MCP server host builder for testing and programmatic use.
    /// </summary>
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddMcpServer()
                    .WithStdioServerTransport()
                    .WithToolsFromAssembly(typeof(WeatherTool).Assembly);
            });
    }
}

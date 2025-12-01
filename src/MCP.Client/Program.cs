using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace MCP.Client;

/// <summary>
/// MCP Client application that connects to MCP servers and invokes tools.
/// </summary>
public static class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("MCP Client - Starting...");
        Console.WriteLine("This client can connect to MCP servers and invoke their tools.");
        Console.WriteLine();

        // For demonstration, show how to create a client
        Console.WriteLine("To connect to an MCP server, use the McpClientFactory class.");
        Console.WriteLine("Example usage is demonstrated in the MCP.Host project.");

        await Task.CompletedTask;
    }
}

/// <summary>
/// Helper class for creating and managing MCP client connections.
/// </summary>
public static class McpClientHelper
{
    /// <summary>
    /// Creates an MCP client that connects to a server via stdio transport.
    /// </summary>
    /// <param name="serverPath">Path to the server executable.</param>
    /// <param name="serverArgs">Optional arguments to pass to the server.</param>
    /// <param name="logger">Optional logger for the client.</param>
    /// <returns>A configured MCP client.</returns>
    public static async Task<IMcpClient> CreateStdioClientAsync(
        string serverPath,
        IList<string>? serverArgs = null,
        ILogger? logger = null)
    {
        var transportOptions = new StdioClientTransportOptions
        {
            Command = serverPath,
            Arguments = serverArgs ?? [],
            Name = "MCP Demo Client"
        };

        var transport = new StdioClientTransport(transportOptions);
        
        var client = await McpClientFactory.CreateAsync(
            transport,
            new McpClientOptions
            {
                ClientInfo = new Implementation
                {
                    Name = "MCP Demo Client",
                    Version = "1.0.0"
                }
            });

        return client;
    }

    /// <summary>
    /// Lists all available tools from an MCP client.
    /// </summary>
    public static async Task<IList<McpClientTool>> GetAvailableToolsAsync(IMcpClient client)
    {
        return await client.ListToolsAsync();
    }

    /// <summary>
    /// Invokes a tool on the MCP server.
    /// </summary>
    public static async Task<CallToolResponse> InvokeToolAsync(
        IMcpClient client,
        string toolName,
        Dictionary<string, object?> arguments)
    {
        return await client.CallToolAsync(toolName, arguments);
    }
}

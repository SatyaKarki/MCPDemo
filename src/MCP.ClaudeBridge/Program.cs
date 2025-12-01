using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Client;
using System.Text.Json;
using ModelContextProtocol.Protocol;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

// Launch MCP server as subprocess using dotnet
string serverPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "src", "MCP.Server", "MCP.Server.dll");
if (!File.Exists(serverPath))
{
 Console.WriteLine($"MCP server not found at {serverPath}. Start the server separately or provide path via environment variable MCP_SERVER_PATH.");
}

// Minimal in-memory client holder
IMcpClient? mcpClient = null;

app.MapGet("/tools", async () =>
{
 if (mcpClient == null)
 return Results.BadRequest("MCP client not connected");

 var tools = await mcpClient.ListToolsAsync();
 return Results.Ok(tools);
});

app.MapPost("/call", async (CallRequest req) =>
{
 if (mcpClient == null)
 return Results.BadRequest("MCP client not connected");

 try
 {
 var result = await mcpClient.CallToolAsync(req.Tool, req.Args ?? new Dictionary<string, object?>());
 return Results.Ok(result);
 }
 catch (Exception ex)
 {
 return Results.Problem(ex.Message);
 }
});

// Start MCP client asynchronously
_ = Task.Run(async () =>
{
 try
 {
 var transportOptions = new StdioClientTransportOptions
 {
 Command = "dotnet",
 Arguments = new[] { serverPath },
 Name = "MCP Claude Bridge"
 };

 var transport = new StdioClientTransport(transportOptions);
 mcpClient = await McpClientFactory.CreateAsync(transport, new McpClientOptions { ClientInfo = new Implementation { Name = "MCP Claude Bridge", Version = "1.0.0" } });
 Console.WriteLine("Connected to MCP server via stdio transport.");
 }
 catch (Exception ex)
 {
 Console.WriteLine($"Failed to start MCP client: {ex.Message}");
 }
});

app.Run();

public record CallRequest(string Tool, Dictionary<string, object?>? Args);

using System.Text.Json;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace MCP.Host;

/// <summary>
/// Host application that demonstrates connecting to an MCP server and using its tools.
/// </summary>
public static class Program
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static async Task Main(string[] args)
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║           MCP Demo Host - Model Context Protocol             ║");
        Console.WriteLine("║                  .NET 10 Implementation                      ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // Determine server path
        var serverPath = GetServerPath(args);
        
        if (string.IsNullOrEmpty(serverPath))
        {
            Console.WriteLine("Usage: MCP.Host [server-path]");
            Console.WriteLine();
            Console.WriteLine("If no server path is provided, it will look for MCP.Server in the default location.");
            Console.WriteLine();
            ShowDemoWithoutServer();
            return;
        }

        await RunDemoWithServerAsync(serverPath);
    }

    private static string? GetServerPath(string[] args)
    {
        if (args.Length > 0 && File.Exists(args[0]))
        {
            return args[0];
        }

        // Try to find the server in common locations
        var possiblePaths = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "MCP.Server.dll"),
            Path.Combine(AppContext.BaseDirectory, "..", "MCP.Server", "MCP.Server.dll"),
            Path.Combine(Directory.GetCurrentDirectory(), "src", "MCP.Server", "bin", "Debug", "net10.0", "MCP.Server.dll"),
            Path.Combine(Directory.GetCurrentDirectory(), "src", "MCP.Server", "bin", "Release", "net10.0", "MCP.Server.dll")
        };

        foreach (var path in possiblePaths)
        {
            var normalizedPath = Path.GetFullPath(path);
            if (File.Exists(normalizedPath))
            {
                return normalizedPath;
            }
        }

        return null;
    }

    private static async Task RunDemoWithServerAsync(string serverPath)
    {
        Console.WriteLine($"🔌 Connecting to MCP Server: {serverPath}");
        Console.WriteLine();

        try
        {
            // Create client connection to server
            var transportOptions = new StdioClientTransportOptions
            {
                Command = "dotnet",
                Arguments = [serverPath],
                Name = "MCP Demo Host"
            };

            var transport = new StdioClientTransport(transportOptions);

            var client = await McpClientFactory.CreateAsync(
                transport,
                new McpClientOptions
                {
                    ClientInfo = new Implementation
                    {
                        Name = "MCP Demo Host",
                        Version = "1.0.0"
                    }
                });

            Console.WriteLine("✅ Connected to MCP Server successfully!");
            Console.WriteLine();

            // List available tools
            await ListToolsAsync(client);

            // Run interactive demo
            await RunInteractiveDemoAsync(client);

            // Cleanup
            await client.DisposeAsync();
            Console.WriteLine("👋 Disconnected from server. Goodbye!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error connecting to server: {ex.Message}");
            Console.WriteLine();
            Console.WriteLine("Make sure the MCP.Server project is built:");
            Console.WriteLine("  dotnet build src/MCP.Server/MCP.Server.csproj");
            Console.WriteLine();
            ShowDemoWithoutServer();
        }
    }

    private static async Task ListToolsAsync(IMcpClient client)
    {
        Console.WriteLine("📋 Available Tools:");
        Console.WriteLine(new string('-', 60));

        var tools = await client.ListToolsAsync();
        foreach (var tool in tools)
        {
            Console.WriteLine($"  • {tool.Name}");
            if (!string.IsNullOrEmpty(tool.Description))
            {
                Console.WriteLine($"    {tool.Description}");
            }
        }

        Console.WriteLine(new string('-', 60));
        Console.WriteLine($"Total: {tools.Count} tools available");
        Console.WriteLine();
    }

    private static async Task RunInteractiveDemoAsync(IMcpClient client)
    {
        Console.WriteLine("🎮 Running Interactive Demo...");
        Console.WriteLine();

        // Demo 1: Weather Tool
        await DemoWeatherTool(client);

        // Demo 2: Calculator Tool
        await DemoCalculatorTool(client);

        // Demo 3: Todo Tool
        await DemoTodoTool(client);

        // Demo 4: Text Utility Tool
        await DemoTextUtilityTool(client);

        // Demo 5: Product Tool
        await DemoProductTool(client);
    }

    private static async Task DemoWeatherTool(IMcpClient client)
    {
        Console.WriteLine("🌤️  Weather Tool Demo");
        Console.WriteLine(new string('-', 40));

        try
        {
            var result = await client.CallToolAsync("GetWeather", new Dictionary<string, object?>
            {
                ["location"] = "Tokyo",
                ["unit"] = "celsius"
            });

            Console.WriteLine($"Weather in Tokyo:");
            PrintToolResult(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ⚠️ Could not get weather: {ex.Message}");
        }

        Console.WriteLine();
    }

    private static async Task DemoCalculatorTool(IMcpClient client)
    {
        Console.WriteLine("🔢 Calculator Tool Demo");
        Console.WriteLine(new string('-', 40));

        try
        {
            // Addition
            var addResult = await client.CallToolAsync("Add", new Dictionary<string, object?>
            {
                ["a"] = 15.5,
                ["b"] = 24.5
            });
            Console.WriteLine("Addition (15.5 + 24.5):");
            PrintToolResult(addResult);

            // Square Root
            var sqrtResult = await client.CallToolAsync("SquareRoot", new Dictionary<string, object?>
            {
                ["number"] = 144
            });
            Console.WriteLine("Square Root (√144):");
            PrintToolResult(sqrtResult);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ⚠️ Calculator error: {ex.Message}");
        }

        Console.WriteLine();
    }

    private static async Task DemoTodoTool(IMcpClient client)
    {
        Console.WriteLine("📝 Todo Tool Demo");
        Console.WriteLine(new string('-', 40));

        try
        {
            // Create a todo
            var createResult = await client.CallToolAsync("CreateTodo", new Dictionary<string, object?>
            {
                ["title"] = "Learn MCP Protocol",
                ["description"] = "Study the Model Context Protocol and implement a demo",
                ["priority"] = "High"
            });
            Console.WriteLine("Created Todo:");
            PrintToolResult(createResult);

            // Create another todo
            await client.CallToolAsync("CreateTodo", new Dictionary<string, object?>
            {
                ["title"] = "Build MCP Server",
                ["description"] = "Implement tools for the MCP server",
                ["priority"] = "Medium"
            });

            // Get stats
            var statsResult = await client.CallToolAsync("GetTodoStats", new Dictionary<string, object?>());
            Console.WriteLine("Todo Statistics:");
            PrintToolResult(statsResult);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ⚠️ Todo error: {ex.Message}");
        }

        Console.WriteLine();
    }

    private static async Task DemoTextUtilityTool(IMcpClient client)
    {
        Console.WriteLine("📄 Text Utility Tool Demo");
        Console.WriteLine(new string('-', 40));

        try
        {
            // Analyze text
            var analyzeResult = await client.CallToolAsync("AnalyzeText", new Dictionary<string, object?>
            {
                ["text"] = "Hello World! This is a demo of the MCP Text Utility Tool. It can analyze text and perform various transformations."
            });
            Console.WriteLine("Text Analysis:");
            PrintToolResult(analyzeResult);

            // Slugify
            var slugResult = await client.CallToolAsync("Slugify", new Dictionary<string, object?>
            {
                ["text"] = "My Awesome Blog Post Title!"
            });
            Console.WriteLine("Slugify 'My Awesome Blog Post Title!':");
            PrintToolResult(slugResult);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ⚠️ Text utility error: {ex.Message}");
        }

        Console.WriteLine();
    }

    private static async Task DemoProductTool(IMcpClient client)
    {
        Console.WriteLine("🛍️ Product Tool Demo");
        Console.WriteLine(new string('-', 40));

        try
        {
            // Create a product
            var createResult = await client.CallToolAsync("CreateProduct", new Dictionary<string, object?>
            {
                ["name"] = "Demo Widget",
                ["price"] = 19.99M,
                ["description"] = "A sample product created by MCP.Host",
                ["isActive"] = true
            });

            Console.WriteLine("Created Product:");
            Console.WriteLine(JsonSerializer.Serialize(createResult, JsonOptions));

            // List products
            var listResult = await client.CallToolAsync("ListProducts", new Dictionary<string, object?>());
            Console.WriteLine("All Products:");
            Console.WriteLine(JsonSerializer.Serialize(listResult, JsonOptions));

            // Try to extract id from created result content if possible
            //long? id = null;
            long? id = 1;
            if (createResult.Content != null && createResult.Content.Count > 0)
            {
                var first = createResult.Content[0];
                if (first.Data != null)
                {
                    try
                    {
                        // Serialize the Data object to JSON and parse for an Id property
                        var json = JsonSerializer.Serialize(first.Data);
                        using var doc = JsonDocument.Parse(json);
                        if (doc.RootElement.TryGetProperty("Id", out var idProp) || doc.RootElement.TryGetProperty("id", out idProp))
                        {
                            if (idProp.ValueKind == JsonValueKind.Number && idProp.TryGetInt64(out var n))
                            {
                                id = n;
                            }
                            else if (idProp.ValueKind == JsonValueKind.String && long.TryParse(idProp.GetString(), out var n2))
                            {
                                id = n2;
                            }
                        }
                    }
                    catch { }
                }
            }

            if (id.HasValue)
            {
                // Get product
                var getResult = await client.CallToolAsync("GetProduct", new Dictionary<string, object?> { ["id"] = id.Value });
                Console.WriteLine($"Get Product {id.Value}:");
                Console.WriteLine(JsonSerializer.Serialize(getResult, JsonOptions));

                // Update product
                var updateResult = await client.CallToolAsync("UpdateProduct", new Dictionary<string, object?>
                {
                    ["id"] = id.Value,
                    ["price"] = 24.99M,
                    ["description"] = "Updated description"
                });
                Console.WriteLine("Updated Product:");
                Console.WriteLine(JsonSerializer.Serialize(updateResult, JsonOptions));

                // Delete product
                var deleteResult = await client.CallToolAsync("DeleteProduct", new Dictionary<string, object?> { ["id"] = id.Value });
                Console.WriteLine("Delete Result:");
                Console.WriteLine(JsonSerializer.Serialize(deleteResult, JsonOptions));

                // List again
                var finalList = await client.CallToolAsync("ListProducts", new Dictionary<string, object?>());
                Console.WriteLine("Products after delete:");
                Console.WriteLine(JsonSerializer.Serialize(finalList, JsonOptions));
            }
            else
            {
                Console.WriteLine("Could not determine created product id from response.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($" ⚠️ Product tool error: {ex.Message}");
        }

        Console.WriteLine();
    }

    private static void PrintToolResult(CallToolResponse result)
    {
        foreach (var content in result.Content)
        {
            if (content.Type == "text" && content.Text != null)
            {
                Console.WriteLine($"  {content.Text}");
            }
        }
    }

    private static void ShowDemoWithoutServer()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║               Demo Mode (No Server Connection)               ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine("This demo shows the available MCP tools and their capabilities:");
        Console.WriteLine();

        Console.WriteLine("🌤️  WEATHER TOOL");
        Console.WriteLine("   • GetWeather - Get current weather for a location");
        Console.WriteLine("   • GetForecast - Get weather forecast for multiple days");
        Console.WriteLine();

        Console.WriteLine("🔢 CALCULATOR TOOL");
        Console.WriteLine("   • Add, Subtract, Multiply, Divide");
        Console.WriteLine("   • Power, SquareRoot");
        Console.WriteLine("   • Percentage, Modulo");
        Console.WriteLine();

        Console.WriteLine("📝 TODO TOOL");
        Console.WriteLine("   • CreateTodo - Create a new task");
        Console.WriteLine("   • GetTodos - List all tasks with filters");
        Console.WriteLine("   • CompleteTodo - Mark a task as done");
        Console.WriteLine("   • UpdateTodo - Modify existing tasks");
        Console.WriteLine("   • DeleteTodo - Remove a task");
        Console.WriteLine("   • GetTodoStats - View task statistics");
        Console.WriteLine("   • ClearCompleted - Remove finished tasks");
        Console.WriteLine();

        Console.WriteLine("📄 TEXT UTILITY TOOL");
        Console.WriteLine("   • AnalyzeText - Word/character/sentence count");
        Console.WriteLine("   • ConvertCase - upper/lower/title/sentence/toggle");
        Console.WriteLine("   • ReverseText - Reverse characters");
        Console.WriteLine("   • RemoveDuplicateLines - Deduplicate text");
        Console.WriteLine("   • ExtractEmails - Find email addresses");
        Console.WriteLine("   • ExtractUrls - Find URLs");
        Console.WriteLine("   • Slugify - Create URL-friendly strings");
        Console.WriteLine("   • Truncate - Shorten text with ellipsis");
        Console.WriteLine();

        Console.WriteLine("🛍️ PRODUCT TOOL");
        Console.WriteLine("   • CreateProduct - Add a new product");
        Console.WriteLine("   • ListProducts - List all products");
        Console.WriteLine("   • GetProduct - Get details of a product by ID");
        Console.WriteLine("   • UpdateProduct - Update product details");
        Console.WriteLine("   • DeleteProduct - Remove a product");
        Console.WriteLine();

        Console.WriteLine("To run the full demo with a live server:");
        Console.WriteLine("  1. Build the solution: dotnet build");
        Console.WriteLine("  2. Run: dotnet run --project src/MCP.Host");
        Console.WriteLine();
    }
}

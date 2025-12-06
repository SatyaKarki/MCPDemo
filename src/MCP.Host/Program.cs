using System.Text.Json;
using System.Globalization;
using System.Linq;
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
        //fetches all MCP tools registered on the server
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
        Console.WriteLine("🎮 Interactive MCP Demo — type 'exit' to quit");
        Console.WriteLine();

        while (true)
        {
            Console.WriteLine("Let’s chat. Which tool would you like to use?");
            Console.WriteLine("  1) WeatherTool");
            Console.WriteLine("  2) CalculatorTool");
            Console.WriteLine("  3) TodoTool");
            Console.WriteLine("  4) TextUtilityTool");
            Console.WriteLine("  5) ProductTool (In-Memory)");
            Console.WriteLine("  6) ProductCatalog (API-Based)");
            Console.Write("Enter choice [1-6] or 'exit': ");

            var input = Console.ReadLine()?.Trim().ToLowerInvariant();
            Console.WriteLine();

            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Please enter a valid choice.");
                Console.WriteLine();
                continue;
            }

            if (input == "exit")
            {
                Console.WriteLine("Exiting interactive demo.");
                Console.WriteLine();
                break;
            }

            switch (input)
            {
                case "1":
                case "weather":
                case "weathertool":
                    await DemoWeatherTool(client);
                    break;

                case "2":
                case "calculator":
                case "calculatortool":
                    await DemoCalculatorTool(client);
                    break;

                case "3":
                case "todo":
                case "todotool":
                    await DemoTodoTool(client);
                    break;

                case "4":
                case "text":
                case "textutility":
                case "textutilitytool":
                    await DemoTextUtilityTool(client);
                    break;

                case "5":
                case "product":
                case "producttool":
                    await DemoProductTool(client);
                    break;

                case "6":
                case "catalog":
                case "productcatalog":
                case "productcatalogtool":
                    await DemoProductCatalogTool(client);
                    break;

                default:
                    Console.WriteLine("Unknown selection. Please choose 1-6 or 'exit'.");
                    Console.WriteLine();
                    break;
            }

            // After each run, ask if they want to continue
            Console.Write("Run another tool? [y/N]: ");
            var again = Console.ReadLine()?.Trim().ToLowerInvariant();
            Console.WriteLine();
            if (again != "y" && again != "yes")
            {
                Console.WriteLine("Leaving interactive demo.");
                Console.WriteLine();
                break;
            }
        }
    }

    private static async Task DemoWeatherTool(IMcpClient client)
    {
        Console.WriteLine("🌤️  Weather Tool");
        Console.WriteLine(new string('-', 40));

        try
        {
            var mode = PromptChoice("What would you like?", new[] { "Current weather", "Forecast" }, 0);
            var location = Prompt("Enter location", "Tokyo");

            if (mode.Equals("Current weather", StringComparison.OrdinalIgnoreCase))
            {
                var unit = PromptChoice("Temperature unit", new[] { "celsius", "fahrenheit" }, 0).ToLowerInvariant();
                var result = await client.CallToolAsync("GetWeather", new Dictionary<string, object?>
                {
                    ["location"] = location,
                    ["unit"] = unit
                });

                Console.WriteLine($"Weather in {location}:");
                PrintToolResult(result);
            }
            else
            {
                var days = PromptInt("How many days of forecast? (1-7)", 3);
                days = Math.Clamp(days, 1, 7);
                var result = await client.CallToolAsync("GetForecast", new Dictionary<string, object?>
                {
                    ["location"] = location,
                    ["days"] = days
                });

                Console.WriteLine($"Forecast for {location} ({days} day(s)):");
                PrintToolResult(result);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ⚠️ Weather error: {ex.Message}");
        }

        Console.WriteLine();
    }

    private static async Task DemoCalculatorTool(IMcpClient client)
    {
        Console.WriteLine("🔢 Calculator Tool");
        Console.WriteLine(new string('-', 40));

        try
        {
            var op = PromptChoice("Choose operation", new[]
            {
                "Add","Subtract","Multiply","Divide","Power","SquareRoot","Percentage","Modulo"
            }, 0);

            CallToolResponse result;
            switch (op.ToLowerInvariant())
            {
                case "add":
                case "subtract":
                case "multiply":
                case "divide":
                case "modulo":
                {
                    var a = PromptDouble("Enter first number", 0);
                    var b = PromptDouble("Enter second number", 0);
                    result = await client.CallToolAsync(Cap(op), new Dictionary<string, object?>
                    {
                        ["a"] = a,
                        ["b"] = b
                    });
                    break;
                }
                case "power":
                {
                    var baseNum = PromptDouble("Enter base", 2);
                    var exp = PromptDouble("Enter exponent", 3);
                    result = await client.CallToolAsync("Power", new Dictionary<string, object?>
                    {
                        ["baseNumber"] = baseNum,
                        ["exponent"] = exp
                    });
                    break;
                }
                case "squareroot":
                case "square root":
                {
                    var number = PromptDouble("Enter number (>= 0)", 144);
                    result = await client.CallToolAsync("SquareRoot", new Dictionary<string, object?>
                    {
                        ["number"] = number
                    });
                    break;
                }
                case "percentage":
                {
                    var part = PromptDouble("Enter part", 50);
                    var whole = PromptDouble("Enter whole", 200);
                    result = await client.CallToolAsync("Percentage", new Dictionary<string, object?>
                    {
                        ["part"] = part,
                        ["whole"] = whole
                    });
                    break;
                }
                default:
                    Console.WriteLine("  ⚠️ Unknown operation.");
                    return;
            }

            PrintToolResult(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ⚠️ Calculator error: {ex.Message}");
        }

        Console.WriteLine();
    }

    private static async Task DemoTodoTool(IMcpClient client)
    {
        Console.WriteLine("📝 Todo Tool");
        Console.WriteLine(new string('-', 40));

        try
        {
            var action = PromptChoice("What would you like to do?", new[]
            {
                "Create","List","Complete","Update","Delete","Stats","ClearCompleted"
            }, 0);

            switch (action.ToLowerInvariant())
            {
                case "create":
                {
                    var title = Prompt("Title", "Learn MCP Protocol");
                    var description = Prompt("Description", "Study the Model Context Protocol and implement a demo");
                    var priority = PromptChoice("Priority", new[] { "Low", "Medium", "High" }, 2);
                    var res = await client.CallToolAsync("CreateTodo", new Dictionary<string, object?>
                    {
                        ["title"] = title,
                        ["description"] = description,
                        ["priority"] = priority
                    });
                    Console.WriteLine("Created Todo:");
                    PrintToolResult(res);
                    break;
                }
                case "list":
                {
                    var filter = PromptChoice("Filter", new[] { "all", "completed", "pending" }, 0).ToLowerInvariant();
                    var priority = PromptChoice("Priority filter", new[] { "all", "Low", "Medium", "High" }, 0);
                    var res = await client.CallToolAsync("GetTodos", new Dictionary<string, object?>
                    {
                        ["filter"] = filter,
                        ["priority"] = priority
                    });
                    PrintToolResult(res);
                    break;
                }
                case "complete":
                {
                    var id = Prompt("Todo ID");
                    var res = await client.CallToolAsync("CompleteTodo", new Dictionary<string, object?>
                    {
                        ["id"] = id
                    });
                    PrintToolResult(res);
                    break;
                }
                case "update":
                {
                    var id = Prompt("Todo ID");
                    var title = Prompt("New title (leave blank to keep)", "");
                    var description = Prompt("New description (leave blank to keep)", null);
                    var priority = Prompt("New priority (Low/Medium/High, leave blank to keep)", "");
                    var payload = new Dictionary<string, object?> { ["id"] = id };
                    if (!string.IsNullOrWhiteSpace(title)) payload["title"] = title;
                    if (description != null) payload["description"] = description;
                    if (!string.IsNullOrWhiteSpace(priority)) payload["priority"] = priority;
                    var res = await client.CallToolAsync("UpdateTodo", payload);
                    PrintToolResult(res);
                    break;
                }
                case "delete":
                {
                    var id = Prompt("Todo ID");
                    var res = await client.CallToolAsync("DeleteTodo", new Dictionary<string, object?>
                    {
                        ["id"] = id
                    });
                    PrintToolResult(res);
                    break;
                }
                case "stats":
                {
                    var res = await client.CallToolAsync("GetTodoStats", new Dictionary<string, object?>());
                    PrintToolResult(res);
                    break;
                }
                case "clearcompleted":
                case "clear":
                {
                    var res = await client.CallToolAsync("ClearCompleted", new Dictionary<string, object?>());
                    PrintToolResult(res);
                    break;
                }
                default:
                    Console.WriteLine("  ⚠️ Unknown action.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ⚠️ Todo error: {ex.Message}");
        }

        Console.WriteLine();
    }

    private static async Task DemoTextUtilityTool(IMcpClient client)
    {
        Console.WriteLine("📄 Text Utility Tool");
        Console.WriteLine(new string('-', 40));

        try
        {
            var action = PromptChoice("Choose action", new[]
            {
                "AnalyzeText","ConvertCase","ReverseText","RemoveDuplicateLines","ExtractEmails","ExtractUrls","Slugify","Truncate"
            }, 0);

            CallToolResponse res;
            switch (action.ToLowerInvariant())
            {
                case "analyzetext":
                {
                    var text = Prompt("Enter text to analyze", "Hello World! This is a sample.");
                    res = await client.CallToolAsync("AnalyzeText", new Dictionary<string, object?> { ["text"] = text });
                    break;
                }
                case "convertcase":
                {
                    var text = Prompt("Enter text", "hello WORLD");
                    var caseType = PromptChoice("Case type", new[] { "upper", "lower", "title", "sentence", "toggle" }, 0);
                    res = await client.CallToolAsync("ConvertCase", new Dictionary<string, object?>
                    {
                        ["text"] = text,
                        ["caseType"] = caseType
                    });
                    break;
                }
                case "reversetext":
                {
                    var text = Prompt("Enter text", "Sample");
                    res = await client.CallToolAsync("ReverseText", new Dictionary<string, object?> { ["text"] = text });
                    break;
                }
                case "removeduplicatelines":
                {
                    var text = Prompt("Enter multi-line text", "a\na\nb");
                    var caseSensitive = PromptBool("Case sensitive?", true);
                    res = await client.CallToolAsync("RemoveDuplicateLines", new Dictionary<string, object?>
                    {
                        ["text"] = text,
                        ["caseSensitive"] = caseSensitive
                    });
                    break;
                }
                case "extractemails":
                {
                    var text = Prompt("Enter text", "Contact us at a@x.com, b@y.io");
                    res = await client.CallToolAsync("ExtractEmails", new Dictionary<string, object?> { ["text"] = text });
                    break;
                }
                case "extracturls":
                {
                    var text = Prompt("Enter text", "https://contoso.com and http://example.org");
                    res = await client.CallToolAsync("ExtractUrls", new Dictionary<string, object?> { ["text"] = text });
                    break;
                }
                case "slugify":
                {
                    var text = Prompt("Enter text", "My Awesome Blog Post Title!");
                    res = await client.CallToolAsync("Slugify", new Dictionary<string, object?> { ["text"] = text });
                    break;
                }
                case "truncate":
                {
                    var text = Prompt("Enter text", "This is a long text that will be truncated.");
                    var maxLen = PromptInt("Max length", 20);
                    var addEllipsis = PromptBool("Add ellipsis?", true);
                    res = await client.CallToolAsync("Truncate", new Dictionary<string, object?>
                    {
                        ["text"] = text,
                        ["maxLength"] = maxLen,
                        ["addEllipsis"] = addEllipsis
                    });
                    break;
                }
                default:
                    Console.WriteLine("  ⚠️ Unknown action.");
                    return;
            }

            PrintToolResult(res);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ⚠️ Text utility error: {ex.Message}");
        }

        Console.WriteLine();
    }

    private static async Task DemoProductTool(IMcpClient client)
    {
        Console.WriteLine("🛍️ Product Tool (In-Memory)");
        Console.WriteLine(new string('-', 40));

        try
        {
            var action = PromptChoice("Choose action", new[] { "Create", "List", "Get", "Update", "Delete" }, 1);

            switch (action.ToLowerInvariant())
            {
                case "create":
                {
                    var name = Prompt("Name", "Demo Widget");
                    var price = PromptDecimal("Price", 19.99m);
                    var description = Prompt("Description", "A sample product created by MCP.Host");
                    var isActive = PromptBool("Is Active?", true);
                    var res = await client.CallToolAsync("CreateProduct", new Dictionary<string, object?>
                    {
                        ["name"] = name,
                        ["price"] = price,
                        ["description"] = description,
                        ["isActive"] = isActive
                    });
                    PrintToolResult(res);
                    break;
                }
                case "list":
                {
                    var res = await client.CallToolAsync("ListProducts", new Dictionary<string, object?>());
                    PrintToolResult(res);
                    break;
                }
                case "get":
                {
                    var id = PromptLong("Product ID", 1);
                    var res = await client.CallToolAsync("GetProduct", new Dictionary<string, object?> { ["id"] = id });
                    PrintToolResult(res);
                    break;
                }
                case "update":
                {
                    var id = PromptLong("Product ID", 1);
                    var name = Prompt("New name (leave blank to keep)", "");
                    var priceStr = Prompt("New price (leave blank to keep)", "");
                    var description = Prompt("New description (leave blank to keep)", null);
                    var isActiveSet = Prompt("New isActive (true/false, leave blank to keep)", "");
                    var payload = new Dictionary<string, object?> { ["id"] = id };
                    if (!string.IsNullOrWhiteSpace(name)) payload["name"] = name;
                    if (!string.IsNullOrWhiteSpace(priceStr) && decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var p)) payload["price"] = p;
                    if (description != null) payload["description"] = description;
                    if (!string.IsNullOrWhiteSpace(isActiveSet) && bool.TryParse(isActiveSet, out var b)) payload["isActive"] = b;
                    var res = await client.CallToolAsync("UpdateProduct", payload);
                    PrintToolResult(res);
                    break;
                }
                case "delete":
                {
                    var id = PromptLong("Product ID", 1);
                    var res = await client.CallToolAsync("DeleteProduct", new Dictionary<string, object?> { ["id"] = id });
                    PrintToolResult(res);
                    break;
                }
                default:
                    Console.WriteLine("  ⚠️ Unknown action.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($" ⚠️ Product tool error: {ex.Message}");
        }

        Console.WriteLine();
    }

    private static async Task DemoProductCatalogTool(IMcpClient client)
    {
        Console.WriteLine("🏪 Product Catalog Tool (API-Based)");
        Console.WriteLine(new string('-', 40));
        Console.WriteLine("Note: Requires ProductApi running on http://localhost:57724");
        Console.WriteLine();

        try
        {
            var action = PromptChoice("Choose action", new[]
            {
                "SearchAll","SearchByTerm","GetById","GetActive","GetByPriceRange","Add","Update","Delete"
            }, 0);

            switch (action.ToLowerInvariant())
            {
                case "searchall":
                {
                    var res = await client.CallToolAsync("SearchProducts", new Dictionary<string, object?>());
                    PrintToolResult(res);
                    break;
                }
                case "searchbyterm":
                {
                    var term = Prompt("Search term", "laptop");
                    var res = await client.CallToolAsync("SearchProducts", new Dictionary<string, object?> { ["searchTerm"] = term });
                    PrintToolResult(res);
                    break;
                }
                case "getbyid":
                {
                    var id = PromptLong("Product ID", 2);
                    var res = await client.CallToolAsync("GetProductById", new Dictionary<string, object?> { ["id"] = id });
                    PrintToolResult(res);
                    break;
                }
                case "getactive":
                {
                    var res = await client.CallToolAsync("GetActiveProducts", new Dictionary<string, object?>());
                    PrintToolResult(res);
                    break;
                }
                case "getbypricerange":
                {
                    var min = PromptDecimal("Min price", 50m);
                    var max = PromptDecimal("Max price", 150m);
                    var res = await client.CallToolAsync("GetProductsByPriceRange", new Dictionary<string, object?>
                    {
                        ["minPrice"] = min,
                        ["maxPrice"] = max
                    });
                    PrintToolResult(res);
                    break;
                }
                case "add":
                {
                    var name = Prompt("Name", "Gaming Mouse Pad");
                    var price = PromptDecimal("Price", 24.99m);
                    var description = Prompt("Description", "Extended RGB gaming mouse pad");
                    var isActive = PromptBool("Is Active?", true);
                    var res = await client.CallToolAsync("AddProduct", new Dictionary<string, object?>
                    {
                        ["name"] = name,
                        ["price"] = price,
                        ["description"] = description,
                        ["isActive"] = isActive
                    });
                    PrintToolResult(res);
                    break;
                }
                case "update":
                {
                    var id = PromptLong("Product ID", 1);
                    var name = Prompt("New name", "Gaming Mouse Pad XL");
                    var price = PromptDecimal("New price", 29.99m);
                    var description = Prompt("New description", "Extended RGB gaming mouse pad - Extra Large");
                    var isActive = PromptBool("Is Active?", true);
                    var res = await client.CallToolAsync("UpdateProduct", new Dictionary<string, object?>
                    {
                        ["id"] = id,
                        ["name"] = name,
                        ["price"] = price,
                        ["description"] = description,
                        ["isActive"] = isActive
                    });
                    PrintToolResult(res);
                    break;
                }
                case "delete":
                {
                    var id = PromptLong("Product ID", 1);
                    var res = await client.CallToolAsync("DeleteProduct", new Dictionary<string, object?> { ["id"] = id });
                    PrintToolResult(res);
                    break;
                }
                default:
                    Console.WriteLine("  ⚠️ Unknown action.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ⚠️ Product Catalog error: {ex.Message}");
            Console.WriteLine("  Make sure ProductApi is running:");
            Console.WriteLine("    cd src\\MCP.ProductApi");
            Console.WriteLine("    dotnet run");
        }

        Console.WriteLine();
    }

    private static long? ExtractProductId(CallToolResponse result)
    {
        if (result.Content != null && result.Content.Count > 0)
        {
            var first = result.Content[0];
            if (first.Data != null)
            {
                try
                {
                    var json = JsonSerializer.Serialize(first.Data);
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("Id", out var idProp) || 
                        doc.RootElement.TryGetProperty("id", out idProp))
                    {
                        if (idProp.ValueKind == JsonValueKind.Number && idProp.TryGetInt64(out var n))
                        {
                            return n;
                        }
                    }
                }
                catch { }
            }
        }
        return null;
    }

    private static void PrintToolResult(CallToolResponse result)
    {
        if (result.Content == null || result.Content.Count == 0)
        {
            Console.WriteLine("  (no content)");
            return;
        }

        foreach (var content in result.Content)
        {
            if (content.Type == "text" && content.Text != null)
            {
                Console.WriteLine($"  {content.Text}");
            }
            else if (content.Data != null)
            {
                Console.WriteLine(JsonSerializer.Serialize(content.Data, JsonOptions));
            }
            else
            {
                Console.WriteLine(JsonSerializer.Serialize(content, JsonOptions));
            }
        }
    }

    // Conversation helpers
    private static string Prompt(string message, string? defaultValue = null)
    {
        if (!string.IsNullOrEmpty(defaultValue))
        {
            Console.Write($"{message} [{defaultValue}]: ");
        }
        else
        {
            Console.Write($"{message}: ");
        }
        var input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input))
        {
            return defaultValue ?? string.Empty;
        }
        return input!.Trim();
    }

    private static string PromptChoice(string message, string[] options, int defaultIndex = 0)
    {
        for (int i = 0; i < options.Length; i++)
        {
            Console.WriteLine($"  {i + 1}) {options[i]}");
        }
        Console.Write($"{message} [{options[defaultIndex]}]: ");
        var input = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(input)) return options[defaultIndex];

        if (int.TryParse(input, out var n) && n >= 1 && n <= options.Length)
            return options[n - 1];

        // match by text
        var match = options.FirstOrDefault(o => o.Equals(input, StringComparison.OrdinalIgnoreCase));
        return match ?? options[defaultIndex];
    }

    private static int PromptInt(string message, int defaultValue)
    {
        Console.Write($"{message} [{defaultValue}]: ");
        var input = Console.ReadLine();
        return int.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out var n) ? n : defaultValue;
    }

    private static long PromptLong(string message, long defaultValue)
    {
        Console.Write($"{message} [{defaultValue}]: ");
        var input = Console.ReadLine();
        return long.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out var n) ? n : defaultValue;
    }

    private static double PromptDouble(string message, double defaultValue)
    {
        Console.Write($"{message} [{defaultValue}]: ");
        var input = Console.ReadLine();
        return double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out var n) ? n : defaultValue;
    }

    private static decimal PromptDecimal(string message, decimal defaultValue)
    {
        Console.Write($"{message} [{defaultValue}]: ");
        var input = Console.ReadLine();
        return decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out var n) ? n : defaultValue;
    }

    private static bool PromptBool(string message, bool defaultValue)
    {
        Console.Write($"{message} [{(defaultValue ? "y" : "n")}]: ");
        var input = Console.ReadLine()?.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(input)) return defaultValue;
        if (input is "y" or "yes" or "true") return true;
        if (input is "n" or "no" or "false") return false;
        return defaultValue;
    }

    private static string Cap(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        if (s.Length == 1) return s.ToUpperInvariant();
        return char.ToUpperInvariant(s[0]) + s[1..];
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

        Console.WriteLine("🛍️ PRODUCT TOOL (In-Memory)");
        Console.WriteLine("   • CreateProduct - Add a new product");
        Console.WriteLine("   • ListProducts - List all products");
        Console.WriteLine("   • GetProduct - Get details of a product by ID");
        Console.WriteLine("   • UpdateProduct - Update product details");
        Console.WriteLine("   • DeleteProduct - Remove a product");
        Console.WriteLine();

        Console.WriteLine("🏪 PRODUCT CATALOG TOOL (API-Based)");
        Console.WriteLine("   • SearchProducts - Search products or list all from API");
        Console.WriteLine("   • GetProductById - Get a specific product from API");
        Console.WriteLine("   • AddProduct - Add new product via API");
        Console.WriteLine("   • UpdateProduct - Update product via API");
        Console.WriteLine("   • DeleteProduct - Delete product via API");
        Console.WriteLine("   • GetActiveProducts - Get only active products");
        Console.WriteLine("   • GetProductsByPriceRange - Filter by price range");
        Console.WriteLine("   Note: Requires ProductApi running on http://localhost:57724");
        Console.WriteLine();

        Console.WriteLine("To run the full demo with a live server:");
        Console.WriteLine("  1. Build the solution: dotnet build");
        Console.WriteLine("  2. Run: dotnet run --project src/MCP.Host");
        Console.WriteLine();
    }
}

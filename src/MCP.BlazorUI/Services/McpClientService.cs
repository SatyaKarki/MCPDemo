using System.Text.Json;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace MCP.BlazorUI.Services;

public class McpClientService : IAsyncDisposable
{
    private IMcpClient? _client;
    private StdioClientTransport? _transport;
    private readonly ILogger<McpClientService> _logger;
    
    public bool IsConnected { get; private set; }
    public List<McpClientTool> AvailableTools { get; private set; } = new();
    public List<string> ConversationHistory { get; private set; } = new();
    
    public event Action? OnConnectionChanged;
    public event Action? OnToolsChanged;
    public event Action? OnConversationChanged;
    
    public McpClientService(ILogger<McpClientService> logger)
    {
        _logger = logger;
    }
    
    public async Task<bool> ConnectAsync(string serverPath)
    {
        try
        {
            if (IsConnected)
            {
                await DisconnectAsync();
            }
            
            _logger.LogInformation("Connecting to MCP Server at: {ServerPath}", serverPath);
            
            var transportOptions = new StdioClientTransportOptions
            {
                Command = "dotnet",
                Arguments = [serverPath],
                Name = "MCP Blazor UI"
            };
            
            _transport = new StdioClientTransport(transportOptions);
            
            _client = await McpClientFactory.CreateAsync(
                _transport,
                new McpClientOptions
                {
                    ClientInfo = new Implementation
                    {
                        Name = "MCP Blazor UI",
                        Version = "1.0.0"
                    }
                });
            
            IsConnected = true;
            _logger.LogInformation("Connected to MCP Server successfully");
            
            await RefreshToolsAsync();
            AddToConversation("System", "Connected to MCP Server successfully!");
            
            OnConnectionChanged?.Invoke();
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to MCP Server");
            AddToConversation("System", $"Error connecting: {ex.Message}");
            IsConnected = false;
            OnConnectionChanged?.Invoke();
            return false;
        }
    }
    
    public async Task DisconnectAsync()
    {
        if (_client != null)
        {
            await _client.DisposeAsync();
            _client = null;
        }
        
        if (_transport != null)
        {
            _transport = null;
        }
        
        IsConnected = false;
        AvailableTools.Clear();
        AddToConversation("System", "Disconnected from MCP Server");
        
        OnConnectionChanged?.Invoke();
        OnToolsChanged?.Invoke();
    }
    
    public async Task RefreshToolsAsync()
    {
        if (_client == null || !IsConnected)
        {
            AvailableTools.Clear();
            OnToolsChanged?.Invoke();
            return;
        }
        
        try
        {
            var tools = await _client.ListToolsAsync();
            AvailableTools = tools.ToList();
            _logger.LogInformation("Loaded {Count} tools", AvailableTools.Count);
            OnToolsChanged?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading tools");
            AddToConversation("System", $"Error loading tools: {ex.Message}");
        }
    }
    
    public async Task<string> CallToolAsync(string toolName, Dictionary<string, object?> arguments)
    {
        if (_client == null || !IsConnected)
        {
            return "Error: Not connected to MCP Server";
        }
        
        try
        {
            AddToConversation("User", $"Calling tool: {toolName} with arguments: {JsonSerializer.Serialize(arguments)}");
            
            var result = await _client.CallToolAsync(toolName, arguments);
            
            var response = FormatToolResponse(result);
            AddToConversation("Assistant", response);
            
            return response;
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error calling tool: {ex.Message}";
            _logger.LogError(ex, "Error calling tool {ToolName}", toolName);
            AddToConversation("Assistant", errorMsg);
            return errorMsg;
        }
    }
    
    private string FormatToolResponse(CallToolResponse result)
    {
        if (result.Content == null || result.Content.Count == 0)
        {
            return "(no content)";
        }
        
        var responses = new List<string>();
        foreach (var content in result.Content)
        {
            if (content.Type == "text" && content.Text != null)
            {
                responses.Add(content.Text);
            }
            else if (content.Data != null)
            {
                var json = JsonSerializer.Serialize(content.Data, new JsonSerializerOptions { WriteIndented = true });
                responses.Add(json);
            }
            else
            {
                responses.Add(JsonSerializer.Serialize(content, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
        
        return string.Join("\n\n", responses);
    }
    
    private void AddToConversation(string speaker, string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        ConversationHistory.Add($"[{timestamp}] {speaker}: {message}");
        OnConversationChanged?.Invoke();
    }
    
    public void ClearConversation()
    {
        ConversationHistory.Clear();
        OnConversationChanged?.Invoke();
    }
    
    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }
}

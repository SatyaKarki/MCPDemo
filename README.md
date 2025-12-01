# MyFirstMCPDemo

A comprehensive learning project demonstrating the **Model Context Protocol (MCP)** implementation with .NET 10. This solution includes an MCP Server with real-world tools, an MCP Client library, and a Host application for testing.

## 📋 Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Running the Demo](#running-the-demo)
- [Available Tools](#available-tools)
- [Testing](#testing)
- [Integration with AI Assistants](#integration-with-ai-assistants)
- [Contributing](#contributing)
- [License](#license)

## 🎯 Overview

The **Model Context Protocol (MCP)** is an open protocol that standardizes how AI applications and tools communicate. This project demonstrates:

- Building an MCP Server that exposes tools to AI assistants
- Creating an MCP Client to connect to servers
- Real-world use cases including weather, calculator, todo management, and text utilities

## 🏗️ Architecture

```
┌─────────────────┐     stdio     ┌─────────────────┐
│   MCP Client    │◄────────────►│   MCP Server    │
│    (Host)       │   transport   │    (Tools)      │
└─────────────────┘               └─────────────────┘
        │                                  │
        │                                  ▼
        │                         ┌───────────────────┐
        │                         │  Weather Tool     │
        │                         │  Calculator Tool  │
        │                         │  Todo Tool        │
        │                         │  Text Utility     │
        │                         └───────────────────┘
        ▼
┌─────────────────┐
│  AI Assistant   │
│  (Claude, etc)  │
└─────────────────┘
```

## 📁 Project Structure

```
MyFirstMCPDemo/
├── src/
│   ├── MCP.Shared/           # Common types and models
│   │   └── Models/
│   │       ├── WeatherInfo.cs
│   │       ├── TodoItem.cs
│   │       └── CalculationResult.cs
│   │
│   ├── MCP.Server/           # MCP Server with tools
│   │   ├── Tools/
│   │   │   ├── WeatherTool.cs
│   │   │   ├── CalculatorTool.cs
│   │   │   ├── TodoTool.cs
│   │   │   └── TextUtilityTool.cs
│   │   └── Program.cs
│   │
│   ├── MCP.Client/           # MCP Client library
│   │   └── Program.cs
│   │
│   └── MCP.Host/             # Demo host application
│       └── Program.cs
│
├── tests/
│   └── MCP.Tests/            # Unit tests
│       └── UnitTest1.cs
│
├── MyFirstMCPDemo.sln
└── README.md
```

## ✅ Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later
- A code editor (VS Code, Visual Studio, Rider, etc.)

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/rijwanansari/MyFirstMCPDemo.git
cd MyFirstMCPDemo
```

### 2. Build the Solution

```bash
dotnet build
```

### 3. Run Tests

```bash
dotnet test
```

## 🎮 Running the Demo

### Run the Host Application

The host application demonstrates connecting to the MCP Server and invoking tools:

```bash
dotnet run --project src/MCP.Host
```

This will:
1. Start the MCP Server
2. Connect to it via stdio transport
3. List all available tools
4. Run interactive demos for each tool

### Run the Server Standalone

To run the MCP Server in standalone mode (for integration with AI assistants):

```bash
dotnet run --project src/MCP.Server
```

## 🛠️ Available Tools

### 🌤️ Weather Tool

Get simulated weather information for any location.

| Tool | Description | Parameters |
|------|-------------|------------|
| `GetWeather` | Get current weather | `location` (string), `unit` (celsius/fahrenheit) |
| `GetForecast` | Get multi-day forecast | `location` (string), `days` (1-7) |

### 🔢 Calculator Tool

Perform mathematical calculations.

| Tool | Description | Parameters |
|------|-------------|------------|
| `Add` | Add two numbers | `a`, `b` |
| `Subtract` | Subtract numbers | `a`, `b` |
| `Multiply` | Multiply numbers | `a`, `b` |
| `Divide` | Divide numbers | `a`, `b` |
| `Power` | Raise to power | `baseNumber`, `exponent` |
| `SquareRoot` | Calculate square root | `number` |
| `Percentage` | Calculate percentage | `part`, `whole` |
| `Modulo` | Calculate remainder | `a`, `b` |

### 📝 Todo Tool

Manage a task list.

| Tool | Description | Parameters |
|------|-------------|------------|
| `CreateTodo` | Create new task | `title`, `description?`, `priority?` |
| `GetTodos` | List tasks | `filter?`, `priority?` |
| `CompleteTodo` | Mark task done | `id` |
| `UpdateTodo` | Modify task | `id`, `title?`, `description?`, `priority?` |
| `DeleteTodo` | Remove task | `id` |
| `GetTodoStats` | Get statistics | - |
| `ClearCompleted` | Remove done tasks | - |

### 📄 Text Utility Tool

Text manipulation and analysis.

| Tool | Description | Parameters |
|------|-------------|------------|
| `AnalyzeText` | Count words, chars, lines | `text` |
| `ConvertCase` | Change text case | `text`, `caseType` |
| `ReverseText` | Reverse characters | `text` |
| `RemoveDuplicateLines` | Deduplicate lines | `text`, `caseSensitive?` |
| `ExtractEmails` | Find email addresses | `text` |
| `ExtractUrls` | Find URLs | `text` |
| `Slugify` | Create URL-friendly string | `text` |
| `Truncate` | Shorten text | `text`, `maxLength`, `addEllipsis?` |

## 🧪 Testing

Run the test suite:

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test class
dotnet test --filter "FullyQualifiedName~CalculatorToolTests"
```

## 🤖 Integration with AI Assistants

### Claude Desktop Configuration

Add this to your Claude Desktop configuration file (`claude_desktop_config.json`):

```json
{
  "mcpServers": {
    "mcp-demo": {
      "command": "dotnet",
      "args": ["run", "--project", "/path/to/MyFirstMCPDemo/src/MCP.Server"]
    }
  }
}
```

### VS Code Extension Configuration

For VS Code with Copilot:

```json
{
  "github.copilot.chat.mcpServers": {
    "mcp-demo": {
      "command": "dotnet",
      "args": ["run", "--project", "/path/to/MyFirstMCPDemo/src/MCP.Server"]
    }
  }
}
```

## 📚 Learning Resources

- [MCP Specification](https://modelcontextprotocol.io/specification)
- [MCP .NET SDK](https://github.com/modelcontextprotocol/csharp-sdk)
- [MCP Documentation](https://modelcontextprotocol.io/docs)

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👤 Author

**Rijwan Ansari**

---

⭐ If you found this project helpful, please give it a star!

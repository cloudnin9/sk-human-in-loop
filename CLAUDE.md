# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a .NET 9 solution implementing an AI-powered flight booking agent with human-in-the-loop decision making. The project consists of two main components:

1. **FlightBookingAgent.Client** - Console application with Semantic Kernel AI agent
2. **FlightBookingAgent.McpServer** - MCP (Model Context Protocol) server providing flight booking tools

## Architecture

The system uses a distributed architecture where:

- The AI agent (client) communicates with the MCP server via STDIO transport
- Semantic Kernel provides the AI orchestration with Ollama as the LLM provider
- Human-in-the-loop interrupts occur at key decision points
- MCP tools handle flight search and booking operations

## Development Commands

### Prerequisites

- .NET 9 SDK (automatically installed in project)
- Ollama running locally at <http://localhost:11434> with qwen3:1.7b model

### Build and Run

```bash
# Build the entire solution
dotnet build

# Run MCP server (in one terminal)
cd FlightBookingAgent.McpServer
dotnet run

# Run AI agent client (in another terminal)
cd FlightBookingAgent.Client
dotnet run
```

### Development Workflow

```bash
# Restore packages
dotnet restore

# Clean build outputs
dotnet clean

# Run with specific Ollama model
OLLAMA_MODEL=llama3.1 dotnet run --project FlightBookingAgent.Client

# Run with custom Ollama endpoint
OLLAMA_ENDPOINT=http://localhost:11434 dotnet run --project FlightBookingAgent.Client
```

## Key Components

### MCP Server Tools

- `FlightSearchTool` - Searches available flights with mock data
- `FlightBookingTool` - Books selected flights and generates confirmations

### AI Agent Services

- `McpClientService` - Semantic Kernel functions that call MCP tools
- `HumanInLoopService` - Handles human approval and input collection
- `FlightBookingService` - Main orchestration with chat completion

### Human-in-Loop Patterns

- Approval required before flight booking actions
- User input collection for passenger details
- Confirmation prompts for final booking decisions

## Environment Variables

- `OLLAMA_ENDPOINT` - Ollama server endpoint (default: <http://localhost:11434>)
- `OLLAMA_MODEL` - Model to use (default: qwen3:1.7b)

## Code Style Guidelines

### String Handling

- **Never use string concatenation** - it's inefficient and hard to read
- **Always use StringBuilder** for multiple string operations
- **Prefer string interpolation** over concatenation or formatting
- **Use multiline strings** whenever possible for better readability
- **Avoid using `\n` or `\n\n` literals** - use `Environment.NewLine` for cross-platform compatibility
- **Use `string.Empty` instead of `""`** - it's more explicit and prevents confusion with null strings

```csharp
// ❌ Bad - String concatenation with \n literals
var message = "Hello " + name + "!\n";
message += "Welcome to " + app + "\n";
message += "Today is " + date;

// ❌ Bad - Using \n literals
var message = $"Hello {name}!\nWelcome to {app}\nToday is {date}";

// ❌ Bad - Using empty string literals
var input = userInput ?? "";
var result = text.Replace("old", "").Trim();

// ✅ Good - StringBuilder with AppendLine (uses Environment.NewLine)
var message = new StringBuilder();
message.AppendLine($"Hello {name}!");
message.AppendLine($"Welcome to {app}");
message.AppendLine($"Today is {date}");

// ✅ Good - Multiline string interpolation
var message = $"""
    Hello {name}!
    Welcome to {app}
    Today is {date}
    """;

// ✅ Good - Explicit Environment.NewLine when needed
var message = $"Hello {name}!{Environment.NewLine}Welcome to {app}";

// ✅ Good - Using string.Empty instead of ""
var input = userInput ?? string.Empty;
var result = text.Replace("old", string.Empty).Trim();
```

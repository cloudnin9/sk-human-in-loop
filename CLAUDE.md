# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a .NET 9 solution implementing an AI-powered flight booking agent with human-in-the-loop decision making. The project consists of two main components:

1. **FlightBookingAgent.Client** - Console application with Semantic Kernel AI agent
2. **FlightBookingAgent.McpServer** - MCP (Model Context Protocol) server providing flight booking tools

## Architecture

The system uses a distributed architecture with intelligent caching and human-in-the-loop decision making:

- **AI Agent (Client)**: Semantic Kernel-powered conversational agent with Ollama LLM running as a background hosted service
- **MCP Server**: Standalone executable providing flight search and booking tools
- **Communication**: STDIO transport between client and MCP server
- **Caching Layer**: FlightCacheService provides thread-safe caching of flight search results
- **Human-in-Loop**: BookFlightConfirmationFilter intercepts booking attempts for user approval
- **Hosted Service**: FlightBookingService runs as BackgroundService with graceful shutdown support via IHostApplicationLifetime
- **Dependency Injection**: Comprehensive DI setup with services, filters, and MCP client factory

## Development Commands

### Prerequisites

- .NET 9 SDK (automatically installed in project)
- Ollama running locally at <http://localhost:11434> with qwen3:1.7b model

### Build and Run

```bash
# Build the entire solution
dotnet build

# Run AI agent client (hosted service will start automatically)
cd FlightBookingAgent.Client
dotnet run --project FlightBookingAgent.Client -- "${PWD}/FlightBookingAgent.McpServer/bin/Debug/net9.0/FlightBookingAgent.McpServer"

# Alternative: Run from solution root
dotnet run --project FlightBookingAgent.Client -- FlightBookingAgent.McpServer/bin/Debug/net9.0/FlightBookingAgent.McpServer

# Graceful shutdown: Press Ctrl+C to stop the hosted service or type 'exit' in the application
```

### Development Workflow

```bash
# Initial setup
dotnet restore

# Development cycle
dotnet clean && dotnet build

# Testing
dotnet test

# Run with different configurations
OLLAMA_MODEL=llama3.1 dotnet run --project FlightBookingAgent.Client
OLLAMA_ENDPOINT=http://localhost:11434 dotnet run --project FlightBookingAgent.Client

# Git workflow
git add .
git commit -m "Your commit message"
git push origin master

# Code formatting and analysis
dotnet format
dotnet build --verbosity normal
```

## Key Components

### MCP Server Tools

- `FlightSearchTool` - Searches available flights with mock data generation
- `FlightBookingTool` - Books selected flights and generates booking confirmations

### AI Agent Services

- `McpClientService` - Semantic Kernel functions that call MCP tools with intelligent caching
- `FlightBookingService` - Background hosted service with conversational AI chat completion, cancellation support, and graceful application shutdown
- `FlightCacheService` - Thread-safe flight data caching using ConcurrentDictionary

### Function Invocation Filters

- `BookFlightConfirmationFilter` - Implements human-in-the-loop pattern for flight booking confirmations

### Models

- `FlightSearchRequest` - Structured request for flight searches
- `FlightOption` - Comprehensive flight details with booking information
- `BookingRequest` - Structured request for flight bookings
- `BookingConfirmation` - Booking confirmation response with details

### Human-in-Loop Patterns

- `BookFlightConfirmationFilter` intercepts booking attempts and requires user approval
- Detailed booking information display before confirmation
- Graceful cancellation handling when user declines
- User confirmation required before any booking actions execute
- Application shutdown triggered by user typing 'exit' via `IHostApplicationLifetime.StopApplication()`

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

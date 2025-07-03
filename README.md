# Flight Booking Agent with Human-in-Loop

This is a .NET 9 solution implementing an AI-powered flight booking agent with human-in-the-loop decision making.

## Quick Start

For detailed setup instructions, development commands, and project architecture, see [CLAUDE.md](./CLAUDE.md).

## Components

- **FlightBookingAgent.Client** - Console application with Semantic Kernel AI agent
- **FlightBookingAgent.McpServer** - MCP (Model Context Protocol) server providing flight booking tools

## Prerequisites

- .NET 9 SDK
- Ollama running locally with qwen3:1.7b model

## Running the Application

```bash
# Build the solution
dotnet build

# Run AI agent client  
cd FlightBookingAgent.Client &&  dotnet run --project FlightBookingAgent.Client.csproj -- "${PWD}/FlightBookingAgent.McpServer/bin/Debug/net9.0/FlightBookingAgent.McpServer"
```

For complete documentation, development guidelines, and architecture details, see [CLAUDE.md](./CLAUDE.md).

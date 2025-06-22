using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

// Add MCP Server
builder.Services.AddMcpServer()
.WithToolsFromAssembly()
.WithStdioServerTransport();

var host = builder.Build();

await host.RunAsync();
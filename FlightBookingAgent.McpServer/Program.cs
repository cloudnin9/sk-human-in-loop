using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;


Log.Logger = new LoggerConfiguration()    
    .WriteTo.File("log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = Host.CreateApplicationBuilder(args);

 
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddSerilog();
});

// Add MCP Server
builder.Services.AddMcpServer()
.WithToolsFromAssembly(typeof(FlightBookingAgent.McpServer.Tools.FlightSearchTool).Assembly)
.WithStdioServerTransport();

var host = builder.Build();

await host.RunAsync();
using FlightBookingAgent.Client.Filters;
using FlightBookingAgent.Client.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Debug);
});

// Add Ollama Chat Completion (default endpoint is http://localhost:11434)
var ollamaEndpoint = Environment.GetEnvironmentVariable("OLLAMA_ENDPOINT") ?? "http://localhost:11434";
var ollamaModel = Environment.GetEnvironmentVariable("OLLAMA_MODEL") ?? "qwen3:1.7b";

#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

builder.Services.AddOllamaChatCompletion(
    modelId: ollamaModel,
    endpoint: new Uri(ollamaEndpoint));

#pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.


// Add application services
builder.Services.AddSingleton<IFlightCacheService, FlightCacheService>();
builder.Services.AddSingleton<McpClientService>();
builder.Services.AddSingleton<FlightBookingService>();
builder.Services.AddSingleton<BookFlightConfirmationFilter>();

// Create the plugin collection (using the KernelPluginFactory to create plugins from objects)
builder.Services.AddSingleton<KernelPluginCollection>((serviceProvider) =>
    [
        KernelPluginFactory.CreateFromObject(serviceProvider.GetRequiredService<McpClientService>())
    ]
);

var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "FlightBookingAgent.McpServer");

var dir = new DirectoryInfo(path);

var clientTransport = new StdioClientTransport(new()
{
    Name = "FlightBookingAgent.McpServer",
    Command = "dotnet",
    Arguments = ["run", "--project", $"{dir.FullName}/FlightBookingAgent.McpServer.csproj"],
});

builder.Services.AddSingleton<IMcpClient>(await CreateMcpClient(clientTransport));

// Finally, create the Kernel service with the service provider and plugin collection
builder.Services.AddTransient((serviceProvider)=> {
    KernelPluginCollection pluginCollection = serviceProvider.GetRequiredService<KernelPluginCollection>();
    var kernel = new Kernel(serviceProvider, pluginCollection);
    
    // Add function invocation filters
    kernel.FunctionInvocationFilters.Add(serviceProvider.GetRequiredService<BookFlightConfirmationFilter>());
    
    return kernel;
});

var host = builder.Build();

var flightService = host.Services.GetRequiredService<FlightBookingService>();

await flightService.StartAsync();

await host.RunAsync();


async Task<IMcpClient> CreateMcpClient(StdioClientTransport clientTransport)
{
    var mcpClient = await McpClientFactory.CreateAsync(clientTransport);
    var tools = await mcpClient.ListToolsAsync();
    foreach (var tool in tools)
    {
        Console.WriteLine($"Connected to server with tools: {tool.Name}");
    }
    return mcpClient;
}
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace FlightBookingAgent.Client.Services;

public class FlightBookingService : BackgroundService
{
    private readonly ILogger<FlightBookingService> _logger;
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;
    private readonly IHostApplicationLifetime _hostLifetime;

    public FlightBookingService(
        ILogger<FlightBookingService> logger,
        Kernel kernel,
        IChatCompletionService chatService,
        IHostApplicationLifetime hostLifetime)
    {
        _logger = logger;
        _kernel = kernel;
        _chatService = chatService;
        _hostLifetime = hostLifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Flight Booking Agent started");
        
        Console.WriteLine("🛫 Welcome to the AI Flight Booking Agent!");
        Console.WriteLine("This intelligent agent will help you search and book flights.");
        Console.WriteLine("\nType 'exit' to quit, or describe what you'd like to do.\n");

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(@"
You are a helpful flight booking assistant. You can search for flights and help book them.
You have access to these tools:
- SearchFlightsAsync: Search for available flights
- BookFlightAsync: Book a specific flight

IMPORTANT RULES:
1. Always search for flights before attempting to book
2. Provide clear, helpful information about flight options
3. Be conversational and friendly
4. Ask for missing information when needed (passenger details, dates, etc.)
");

        while (!stoppingToken.IsCancellationRequested)
        {
            Console.Write("You: ");
            var userInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(userInput) || string.Equals(userInput, "exit", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("\n✈️ Thank you for using the Flight Booking Agent. Safe travels!");
                _hostLifetime.StopApplication();
                break;
            }

            try
            {
                chatHistory.AddUserMessage(userInput);

                // Get AI response with function calling
                var response = await _chatService.GetChatMessageContentAsync(
                    chatHistory,
                    new PromptExecutionSettings
                    {
                        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),                         
                    },
                    _kernel,
                    stoppingToken);

                Console.WriteLine($"Assistant: {response.Content}\n");
                chatHistory.AddAssistantMessage(response.Content ?? string.Empty);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing user request");
                Console.WriteLine($"Assistant: I encountered an error: {ex.Message}\nPlease try again.\n");
            }
        }
    }   
}
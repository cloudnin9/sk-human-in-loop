using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace FlightBookingAgent.Client.Services;

public class FlightBookingService
{
    private readonly ILogger<FlightBookingService> _logger;
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;
    private readonly McpClientService _mcpClient;
    private readonly HumanInLoopService _humanInLoop;

    public FlightBookingService(
        ILogger<FlightBookingService> logger,
        Kernel kernel,
        IChatCompletionService chatService,
        McpClientService mcpClient,
        HumanInLoopService humanInLoop)
    {
        _logger = logger;
        _kernel = kernel;
        _chatService = chatService;
        _mcpClient = mcpClient;
        _humanInLoop = humanInLoop;
    }

    public async Task StartAsync()
    {
        _logger.LogInformation("Flight Booking Agent with Human-in-the-Loop started");
        
        Console.WriteLine("🛫 Welcome to the AI Flight Booking Agent!");
        Console.WriteLine("This intelligent agent will help you search and book flights.");
        Console.WriteLine("Human approval is required for all booking actions.");
        Console.WriteLine("\nType 'exit' to quit, or describe what you'd like to do.\n");

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(@"
You are a helpful flight booking assistant. You can search for flights and help book them.
You have access to these tools:
- SearchFlightsAsync: Search for available flights
- BookFlightAsync: Book a specific flight

IMPORTANT RULES:
1. Before booking any flight, you MUST get explicit human approval
2. Always search for flights before attempting to book
3. Provide clear, helpful information about flight options
4. Be conversational and friendly
5. Ask for missing information when needed (passenger details, dates, etc.)
");

        while (true)
        {
            Console.Write("You: ");
            var userInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(userInput) || userInput.ToLower() == "exit")
            {
                Console.WriteLine("\n✈️ Thank you for using the Flight Booking Agent. Safe travels!");
                break;
            }

            try
            {
                chatHistory.AddUserMessage(userInput);

                // Check if this is a booking request that needs human approval
                if (HumanInLoopService.ContainsBookingIntent(userInput))
                {
                    var shouldProceed = _humanInLoop.ShouldProceedWithActionAsync(
                        "Flight Booking Request",
                        $"User wants to: {userInput}\n\nThis will involve searching and potentially booking flights.");

                    if (!shouldProceed)
                    {
                        Console.WriteLine("Assistant: I understand. Let me know if you'd like to do something else.\n");
                        continue;
                    }
                }

                // Get AI response with function calling
                var response = await _chatService.GetChatMessageContentAsync(
                    chatHistory,
                    new PromptExecutionSettings
                    {
                        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                    },
                    _kernel);

                Console.WriteLine($"Assistant: {response.Content}\n");
                chatHistory.AddAssistantMessage(response.Content ?? "");

                // If the AI wants to book a flight, get additional human approval
                if (HumanInLoopService.ContainsBookingIntent(response.Content ?? ""))
                {
                    var shouldBook = _humanInLoop.ShouldProceedWithActionAsync(
                        "Final Booking Confirmation",
                        "The AI is about to proceed with the flight booking. This action will charge your payment method.");

                    if (!shouldBook)
                    {
                        Console.WriteLine("Assistant: Booking cancelled. Is there anything else I can help you with?\n");
                        chatHistory.AddAssistantMessage("Booking cancelled per user request.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing user request");
                Console.WriteLine($"Assistant: I encountered an error: {ex.Message}\nPlease try again.\n");
            }
        }
    }

   
}
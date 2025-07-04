using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.Text;

namespace FlightBookingAgent.Client.Filters;

public class BookFlightConfirmationFilter : IFunctionInvocationFilter
{
    private const string UnknownValue = "Unknown";
    private readonly ILogger<BookFlightConfirmationFilter> _logger;

    public BookFlightConfirmationFilter(ILogger<BookFlightConfirmationFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        // Only apply filter to BookFlight function
        if (context.Function.Name != "BookFlight")
        {
            await next(context);
            return;
        }

        _logger.LogInformation("Function invocation filter triggered for BookFlight");

        // Extract arguments for display
        var flightNumber = context.Arguments.GetValueOrDefault("flightNumber")?.ToString() ?? UnknownValue;
        var passengerName = context.Arguments.GetValueOrDefault("passengerName")?.ToString() ?? UnknownValue;
        var email = context.Arguments.GetValueOrDefault("email")?.ToString() ?? UnknownValue;
        var phone = context.Arguments.GetValueOrDefault("phone")?.ToString() ?? UnknownValue;

        // Display booking confirmation prompt
        var prompt = new StringBuilder();
        prompt.AppendLine();
        prompt.AppendLine("🔔 BOOKING CONFIRMATION REQUIRED");
        prompt.AppendLine("=====================================");
        prompt.AppendLine($"Flight Number: {flightNumber}");
        prompt.AppendLine($"Passenger Name: {passengerName}");
        prompt.AppendLine($"Email: {email}");
        prompt.AppendLine($"Phone: {phone}");
        prompt.AppendLine();
        prompt.AppendLine("⚠️  This will book the flight and may incur charges.");
        prompt.AppendLine("Do you want to proceed with this booking? (y/N): ");

        Console.Write(prompt.ToString());
        
        var response = Console.ReadLine()?.Trim().ToLowerInvariant();
        
        if (response != "y" && response != "yes")
        {
            _logger.LogInformation("User cancelled flight booking for {FlightNumber}", flightNumber);
            
            // Cancel the function execution by throwing an exception
            throw new InvalidOperationException("Flight booking cancelled by user.");
        }

        // Proceed with the function execution
        await next(context);
        
        // Log completion
        _logger.LogInformation("Flight booking confirmed and completed for {FlightNumber}", flightNumber);
    }
}
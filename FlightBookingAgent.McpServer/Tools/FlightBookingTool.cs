using System.ComponentModel;
using FlightBookingAgent.McpServer.Models;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace FlightBookingAgent.McpServer.Tools;

[McpServerToolType()]
public class FlightBookingTool
{
    private readonly ILogger<FlightBookingTool> _logger;

    public FlightBookingTool(ILogger<FlightBookingTool> logger)
    {
        _logger = logger;
    }

    [McpServerTool, Description("Flight booking with passenger details")]
    public async Task<BookingConfirmation> BookFlight(BookingRequest request)
    {
        _logger.LogInformation("Booking flight {FlightNumber} for passenger {Name}", 
            request.Flight.FlightNumber, request.PassengerName);

        // Simulate booking process
        await Task.Delay(2000); // Simulate API call delay

        var bookingReference = GenerateBookingReference();
        
        var confirmation = new BookingConfirmation(
            BookingReference: bookingReference,
            Flight: request.Flight,
            PassengerName: request.PassengerName,
            TotalPrice: request.Flight.Price,
            BookedAt: DateTime.UtcNow
        );

        _logger.LogInformation("Flight booked successfully. Booking reference: {Reference}", bookingReference);
        return confirmation;
    }

    private static string GenerateBookingReference()
    {
        return Guid.NewGuid().ToString("N").AsSpan(0, 8).ToString();
    }
}
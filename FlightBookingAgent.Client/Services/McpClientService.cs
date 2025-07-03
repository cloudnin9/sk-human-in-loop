using FlightBookingAgent.Client.Models;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;
using System.ComponentModel;
using System.Text.Json;
using System.Text;

namespace FlightBookingAgent.Client.Services;

public class McpClientService
{
    private const string BookingErrorMessageTemplate = "Error booking flight";
    private const string SearchFlightsErrorMessageTemplate = "Error looking for flights";
    private const string BookingAttemptMessageTemplate = "Attempting to book flight {FlightNumber} for {Name}";
    private const string BookingFailedMessageTemplate = "Failed to book flight. Please try again.";
    private const string SearchingFlightsMessageTemplate = "Searching flights from {Origin} to {Destination} on {Date}";
    private const string DateFormatValidationErrorMessageTemplate = "Invalid date format. Please use YYYY-MM-DD format.";
    private const string NoFlightsFoundMessageTemplate = "No flights found for the specified criteria.";
    private const string DateTimeDisplayFormat = "MMM dd, yyyy HH:mm";
    private readonly ILogger<McpClientService> _logger;
    private readonly IMcpClient _mcpClient;

    public McpClientService(ILogger<McpClientService> logger, IMcpClient mcpClient)
    {
        _logger = logger;
        _mcpClient = mcpClient;
    }

    [KernelFunction]
    [Description("Search for available flights between two locations, departure date is required and return date should be provided if possible but is optional. Number of passengers will defaults to 1 when it is not explcitly provided.")]
    public async Task<string> SearchFlights(
        [Description("Origin airport code")] string origin,
        [Description("Destination airport code")] string destination,
        [Description("Departure date in YYYY-MM-DD format")] string departureDate,
        [Description("Optional Return date in YYYY-MM-DD format")] string? returnDate = null,
        [Description("Optional Number of passengers which will default to 1 passenger")] int passengers = 1)
    {
        try
        {
            _logger.LogInformation(SearchingFlightsMessageTemplate, origin, destination, departureDate);

            if (!DateOnly.TryParse(departureDate, out var dDate))
            {
                return DateFormatValidationErrorMessageTemplate;
            }

            DateOnly? rDate = null;
            if (!string.IsNullOrWhiteSpace(returnDate))
            {
                if (!DateOnly.TryParse(returnDate, out var parsedReturnDate))
                {
                    return DateFormatValidationErrorMessageTemplate;
                }
                rDate = parsedReturnDate;
            }

            var request = new FlightSearchRequest(origin, destination, dDate, rDate, passengers);

            ModelContextProtocol.Protocol.CallToolResult result = await _mcpClient.CallToolAsync("SearchFlights", new Dictionary<string, object?>
            {
                { "request", request }
            });

            if (result != null && result.IsError) throw new ApplicationException($"{(result.Content.FirstOrDefault() as ModelContextProtocol.Protocol.TextContentBlock)?.Text ?? "Unknown error"}");

            var textContent = result!.Content
                            .Where(c => c.Type == "text")
                            .Cast<ModelContextProtocol.Protocol.TextContentBlock>()
                            .Select(c => c.Text)
                            .FirstOrDefault();

            var flights = JsonSerializer.Deserialize<IEnumerable<FlightOption>>(textContent ?? "[]");

            if (flights?.Any() == true)
            {
                var flightList = flights.ToList();
                var response = new StringBuilder($"Found {flightList.Count} available flights:");
                response.AppendLine();
                response.AppendLine();

                for (int i = 0; i < flightList.Count; i++)
                {
                    var flight = flightList[i];
                    response.AppendLine($"{i + 1}. {flight.Airline} {flight.FlightNumber}");
                    response.AppendLine($"   Route: {flight.Origin} → {flight.Destination}");
                    response.AppendLine($"   Departure: {flight.DepartureTime.ToString(DateTimeDisplayFormat)}");
                    response.AppendLine($"   Arrival: {flight.ArrivalTime.ToString(DateTimeDisplayFormat)}");
                    response.AppendLine($"   Price: ${flight.Price:F2}");
                    response.AppendLine($"   Aircraft: {flight.Aircraft}");
                    response.AppendLine();
                }

                return response.ToString();
            }
            return NoFlightsFoundMessageTemplate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, SearchFlightsErrorMessageTemplate);
            return $"{SearchFlightsErrorMessageTemplate}: {ex.Message}";
        }
    }

    [KernelFunction]
    [Description("Book a specific flight for a passenger")]
    public async Task<string> BookFlight(
        [Description("Flight number to book")] string flightNumber,
        [Description("Passenger full name")] string passengerName,
        [Description("Passenger email address")] string email,
        [Description("Passenger phone number")] string phone)
    {
        try
        {
            _logger.LogInformation(BookingAttemptMessageTemplate, flightNumber, passengerName);

            // Note: In a real implementation, you'd need to get the full flight details
            // For this demo, we'll create a mock flight object
            var mockFlight = new FlightOption(
                FlightNumber: flightNumber,
                Airline: "Demo Airline",
                Origin: "JFK",
                Destination: "LAX",
                DepartureTime: DateTime.Now.AddDays(7),
                ArrivalTime: DateTime.Now.AddDays(7).AddHours(5),
                Price: 299.99m,
                Aircraft: "Boeing 737"
            );

            var request = new BookingRequest(mockFlight, passengerName, email, phone);
            var result = await _mcpClient.CallToolAsync("BookFlights", new Dictionary<string, object?>
            {
                { "request", request }
            });

            if (result != null)
            {
                var confirmation = JsonSerializer.Deserialize<BookingConfirmation>(result.ToString()!);
                if (confirmation != null)
                {
                    return
                    $"""
                    ✅ Flight booked successfully!
                    
                    Booking Reference: {confirmation.BookingReference}
                    Flight: {confirmation.Flight.Airline} {confirmation.Flight.FlightNumber}
                    Passenger: {confirmation.PassengerName}
                    Total Price: ${confirmation.TotalPrice:F2}
                    Booked At: {confirmation.BookedAt.ToString(DateTimeDisplayFormat)} UTC"
                    """;
                }
            }

            return BookingFailedMessageTemplate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, BookingErrorMessageTemplate);
            return $"{BookingErrorMessageTemplate}: {ex.Message}";
        }
    }
}
using FlightBookingAgent.Client.Models;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;
using System.ComponentModel;
using System.Text.Json;
using System.Text;
using ModelContextProtocol.Protocol;

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
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };
    private readonly ILogger<McpClientService> _logger;
    private readonly IMcpClient _mcpClient;
    private readonly IFlightCacheService _flightCacheService;

    public McpClientService(ILogger<McpClientService> logger, IMcpClient mcpClient, IFlightCacheService flightCacheService)
    {
        _logger = logger;
        _mcpClient = mcpClient;
        _flightCacheService = flightCacheService;
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

            if (!DateOnly.TryParse(departureDate, System.Globalization.CultureInfo.InvariantCulture, out var dDate))
            {
                return DateFormatValidationErrorMessageTemplate;
            }

            DateOnly? rDate = null;
            if (!string.IsNullOrWhiteSpace(returnDate))
            {
                if (!DateOnly.TryParse(returnDate, System.Globalization.CultureInfo.InvariantCulture, out var parsedReturnDate))
                {
                    return DateFormatValidationErrorMessageTemplate;
                }
                rDate = parsedReturnDate;
            }

            var request = new FlightSearchRequest(origin, destination, dDate, rDate, passengers);

            CallToolResult result = await _mcpClient.CallToolAsync("SearchFlights", new Dictionary<string, object?>
            {
                { "request", request }
            });

            if (result != null && result.IsError) throw new InvalidOperationException($"{(result.Content.FirstOrDefault() as ModelContextProtocol.Protocol.TextContentBlock)?.Text ?? "Unknown error"}");

            var textContent = result!.Content
                            .Where(c => c.Type == "text")
                            .Cast<TextContentBlock>()
                            .Select(c => c.Text)
                            .FirstOrDefault();

            var flights = JsonSerializer.Deserialize<FlightOption[]>(textContent ?? "[]", JsonOptions)!;

            // Cache the flight results
            _flightCacheService.CacheFlights(flights);
            _logger.LogInformation("Cached {Count} flights from search results", flights.Length);

            if (flights.Length > 0)
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

            // Look up flight details from cache
            var selectedFlight = _flightCacheService.GetCachedFlight(flightNumber);
            if (selectedFlight == null)
            {
                _logger.LogWarning("Flight {FlightNumber} not found in cache", flightNumber);
                return $"Flight {flightNumber} not found. Please search for flights first.";
            }

            var request = new BookingRequest(selectedFlight, passengerName, email, phone);
            var result = await _mcpClient.CallToolAsync("BookFlight", new Dictionary<string, object?>
            {
                { "request", request }
            });

            var confirmation = JsonSerializer.Deserialize<BookingConfirmation>(result.Content.Cast<TextContentBlock>().Select(c => c.Text).FirstOrDefault()!, JsonOptions);

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
            return BookingFailedMessageTemplate;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("cancelled by user"))
        {
            _logger.LogInformation(ex, "Flight booking cancelled by user for {FlightNumber}", flightNumber);
            return "Flight booking was cancelled by user.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, BookingErrorMessageTemplate);
            return $"{BookingErrorMessageTemplate}: {ex.Message}";
        }
    }
}
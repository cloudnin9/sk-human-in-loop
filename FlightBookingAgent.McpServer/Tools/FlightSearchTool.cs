using System.ComponentModel;
using FlightBookingAgent.McpServer.Models;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace FlightBookingAgent.McpServer.Tools;

[McpServerToolType()]    
public class FlightSearchTool
{
    private readonly ILogger<FlightSearchTool> _logger;

    public FlightSearchTool(ILogger<FlightSearchTool> logger)
    {
        _logger = logger;
    }

    [McpServerTool, Description("Search for available flights based on the provided criteria.")]
    public async Task<IEnumerable<FlightOption>> SearchFlights(FlightSearchRequest request)
    {
        _logger.LogInformation("Searching flights from {Origin} to {Destination} on {Date}",
            request.Origin, request.Destination, request.DepartureDate);

        // Simulate flight search with mock data
        await Task.Delay(1000); // Simulate API call delay

        var flights = new List<FlightOption>
        {
            new FlightOption(
                FlightNumber: "AA101",
                Airline: "American Airlines",
                Origin: request.Origin,
                Destination: request.Destination,
                DepartureTime: request.DepartureDate.ToDateTime(new TimeOnly(8, 30)),
                ArrivalTime: request.DepartureDate.ToDateTime(new TimeOnly(11, 45)),
                Price: 299.99m,
                Aircraft: "Boeing 737"
            ),
            new FlightOption(
                FlightNumber: "DL205",
                Airline: "Delta Air Lines",
                Origin: request.Origin,
                Destination: request.Destination,
                DepartureTime: request.DepartureDate.ToDateTime(new TimeOnly(14, 15)),
                ArrivalTime: request.DepartureDate.ToDateTime(new TimeOnly(17, 30)),
                Price: 349.99m,
                Aircraft: "Airbus A320"
            ),
            new FlightOption(
                FlightNumber: "UA308",
                Airline: "United Airlines",
                Origin: request.Origin,
                Destination: request.Destination,
                DepartureTime: request.DepartureDate.ToDateTime(new TimeOnly(19, 45)),
                ArrivalTime: request.DepartureDate.ToDateTime(new TimeOnly(23, 00)),
                Price: 279.99m,
                Aircraft: "Boeing 737 MAX"
            )
        };

        _logger.LogInformation("Found {Count} available flights", flights.Count);
        return flights;
    }
}
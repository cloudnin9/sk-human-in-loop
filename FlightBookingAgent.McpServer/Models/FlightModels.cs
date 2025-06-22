namespace FlightBookingAgent.McpServer.Models;

public record FlightSearchRequest(
    string Origin,
    string Destination,
    DateOnly DepartureDate,
    DateOnly? ReturnDate = null,
    int Passengers = 1
);

public record FlightOption(
    string FlightNumber,
    string Airline,
    string Origin,
    string Destination,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    decimal Price,
    string Aircraft
);

public record BookingRequest(
    FlightOption Flight,
    string PassengerName,
    string Email,
    string Phone
);

public record BookingConfirmation(
    string BookingReference,
    FlightOption Flight,
    string PassengerName,
    decimal TotalPrice,
    DateTime BookedAt
);
namespace FlightBookingAgent.Client.Models;

public record FlightSearchRequest(
    string Origin,
    string Destination,
    DateOnly DepartureDate,
    DateOnly? ReturnDate = null,
    int Passengers = 1
)
{
    public IReadOnlyDictionary<string, object> ToDictionary()
    {
        return new Dictionary<string, object>
        {
            ["origin"] = Origin,
            ["destination"] = Destination,
            ["departureDate"] = DepartureDate.ToString("yyyy-MM-dd"),
            ["returnDate"] = ReturnDate?.ToString("yyyy-MM-dd") ?? string.Empty,
            ["passengers"] = Passengers
        };
    }
}

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
)
{
    public IReadOnlyDictionary<string, object?> ToDictionary()
    {
        return new Dictionary<string, object?>
        {
            ["flight"] = new Dictionary<string, object>
            {
                ["flightNumber"] = Flight.FlightNumber,
                ["airline"] = Flight.Airline,
                ["origin"] = Flight.Origin,
                ["destination"] = Flight.Destination,
                ["departureTime"] = Flight.DepartureTime.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                ["arrivalTime"] = Flight.ArrivalTime.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                ["price"] = Flight.Price,
                ["aircraft"] = Flight.Aircraft
            },
            ["passengerName"] = PassengerName,
            ["email"] = Email,
            ["phone"] = Phone
        };
    }
}

public record BookingConfirmation(
    string BookingReference,
    FlightOption Flight,
    string PassengerName,
    decimal TotalPrice,
    DateTime BookedAt
);
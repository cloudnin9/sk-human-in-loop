using FlightBookingAgent.Client.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace FlightBookingAgent.Client.Services;

public interface IFlightCacheService
{
    void CacheFlights(FlightOption[] flights);
    FlightOption? GetCachedFlight(string flightNumber);
    void ClearFlightCache();
    int GetCachedFlightCount();
    IReadOnlyCollection<string> GetCachedFlightNumbers();
}

public class FlightCacheService : IFlightCacheService
{
    private readonly ILogger<FlightCacheService> _logger;
    private readonly ConcurrentDictionary<string, FlightOption> _flightCache = new();

    public FlightCacheService(ILogger<FlightCacheService> logger)
    {
        _logger = logger;
    }

    public void CacheFlights(FlightOption[] flights)
    {
        foreach (var flight in flights)
        {
            _flightCache[flight.FlightNumber] = flight;
            _logger.LogDebug("Cached flight {FlightNumber}", flight.FlightNumber);
        }
    }

    public FlightOption? GetCachedFlight(string flightNumber)
    {
        return _flightCache.TryGetValue(flightNumber, out var flight) ? flight : null;
    }

    public void ClearFlightCache()
    {
        var count = _flightCache.Count;
        _flightCache.Clear();
        _logger.LogInformation("Cleared flight cache containing {Count} flights", count);
    }

    public int GetCachedFlightCount()
    {
        return _flightCache.Count;
    }

    public IReadOnlyCollection<string> GetCachedFlightNumbers()
    {
        return [.. _flightCache.Keys];
    }
}
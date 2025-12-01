namespace MCP.Shared.Models;

/// <summary>
/// Represents weather information for a location.
/// </summary>
public record WeatherInfo(
    string Location,
    double Temperature,
    string Unit,
    string Condition,
    int Humidity,
    double WindSpeed,
    DateTime Timestamp
);

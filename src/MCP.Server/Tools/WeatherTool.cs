using System.ComponentModel;
using MCP.Shared.Models;
using ModelContextProtocol.Server;

namespace MCP.Server.Tools;

/// <summary>
/// MCP Tool providing weather information for various locations.
/// This is a demonstration tool that returns simulated weather data.
/// </summary>
[McpServerToolType]
public static class WeatherTool
{
    private static readonly Dictionary<string, (double Lat, double Lon)> KnownLocations = new()
    {
        ["new york"] = (40.7128, -74.0060),
        ["london"] = (51.5074, -0.1278),
        ["tokyo"] = (35.6762, 139.6503),
        ["paris"] = (48.8566, 2.3522),
        ["sydney"] = (-33.8688, 151.2093),
        ["mumbai"] = (19.0760, 72.8777),
        ["dubai"] = (25.2048, 55.2708),
        ["singapore"] = (1.3521, 103.8198)
    };

    private static readonly string[] Conditions = ["Sunny", "Cloudy", "Partly Cloudy", "Rainy", "Stormy", "Foggy", "Windy"];

    /// <summary>
    /// Gets the current weather for a specified location.
    /// </summary>
    /// <param name="location">The city name to get weather for (e.g., "New York", "London", "Tokyo").</param>
    /// <param name="unit">Temperature unit: "celsius" or "fahrenheit". Defaults to celsius.</param>
    /// <returns>Weather information including temperature, conditions, humidity, and wind speed.</returns>
    [McpServerTool, Description("Gets the current weather for a specified location. Returns temperature, conditions, humidity, and wind speed.")]
    public static WeatherInfo GetWeather(
        [Description("The city name to get weather for (e.g., 'New York', 'London', 'Tokyo')")] string location,
        [Description("Temperature unit: 'celsius' or 'fahrenheit'. Defaults to celsius.")] string unit = "celsius")
    {
        var normalizedLocation = location.ToLowerInvariant().Trim();
        var random = new Random(normalizedLocation.GetHashCode() + DateTime.UtcNow.DayOfYear);

        // Generate realistic temperature based on location type
        double baseTemp = KnownLocations.ContainsKey(normalizedLocation)
            ? GetBaseTemperatureForLocation(normalizedLocation)
            : random.Next(5, 30);

        var temperature = baseTemp + random.Next(-5, 5);
        var condition = Conditions[random.Next(Conditions.Length)];
        var humidity = random.Next(30, 90);
        var windSpeed = Math.Round(random.NextDouble() * 30, 1);

        if (unit.ToLowerInvariant() == "fahrenheit")
        {
            temperature = Math.Round(temperature * 9 / 5 + 32, 1);
        }
        else
        {
            temperature = Math.Round(temperature, 1);
        }

        return new WeatherInfo(
            Location: location,
            Temperature: temperature,
            Unit: unit.ToLowerInvariant() == "fahrenheit" ? "°F" : "°C",
            Condition: condition,
            Humidity: humidity,
            WindSpeed: windSpeed,
            Timestamp: DateTime.UtcNow
        );
    }

    /// <summary>
    /// Gets the weather forecast for a location for the next specified number of days.
    /// </summary>
    /// <param name="location">The city name to get the forecast for.</param>
    /// <param name="days">Number of days for the forecast (1-7).</param>
    /// <returns>A list of weather forecasts for the specified days.</returns>
    [McpServerTool, Description("Gets weather forecast for a location for the next specified number of days (1-7).")]
    public static List<WeatherInfo> GetForecast(
        [Description("The city name to get the forecast for")] string location,
        [Description("Number of days for the forecast (1-7)")] int days = 3)
    {
        days = Math.Clamp(days, 1, 7);
        var forecasts = new List<WeatherInfo>();

        for (int i = 0; i < days; i++)
        {
            var futureDate = DateTime.UtcNow.AddDays(i);
            var random = new Random(location.GetHashCode() + futureDate.DayOfYear);

            var temperature = Math.Round(random.Next(5, 30) + random.NextDouble() * 5, 1);
            var condition = Conditions[random.Next(Conditions.Length)];
            var humidity = random.Next(30, 90);
            var windSpeed = Math.Round(random.NextDouble() * 30, 1);

            forecasts.Add(new WeatherInfo(
                Location: location,
                Temperature: temperature,
                Unit: "°C",
                Condition: condition,
                Humidity: humidity,
                WindSpeed: windSpeed,
                Timestamp: futureDate
            ));
        }

        return forecasts;
    }

    private static double GetBaseTemperatureForLocation(string location) => location switch
    {
        "tokyo" => 18,
        "london" => 12,
        "new york" => 15,
        "paris" => 14,
        "sydney" => 22,
        "mumbai" => 30,
        "dubai" => 35,
        "singapore" => 28,
        _ => 20
    };
}

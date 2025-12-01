using System.ComponentModel;
using MCP.Shared.Models;
using ModelContextProtocol.Server;

namespace MCP.Server.Tools;

/// <summary>
/// MCP Tool providing calculator operations for arithmetic calculations.
/// </summary>
[McpServerToolType]
public static class CalculatorTool
{
    /// <summary>
    /// Adds two numbers together.
    /// </summary>
    [McpServerTool, Description("Adds two numbers together and returns the sum.")]
    public static CalculationResult Add(
        [Description("The first number")] double a,
        [Description("The second number")] double b)
    {
        var result = a + b;
        return new CalculationResult($"{a} + {b}", result, "Addition", DateTime.UtcNow);
    }

    /// <summary>
    /// Subtracts the second number from the first number.
    /// </summary>
    [McpServerTool, Description("Subtracts the second number from the first number.")]
    public static CalculationResult Subtract(
        [Description("The first number")] double a,
        [Description("The second number")] double b)
    {
        var result = a - b;
        return new CalculationResult($"{a} - {b}", result, "Subtraction", DateTime.UtcNow);
    }

    /// <summary>
    /// Multiplies two numbers together.
    /// </summary>
    [McpServerTool, Description("Multiplies two numbers together and returns the product.")]
    public static CalculationResult Multiply(
        [Description("The first number")] double a,
        [Description("The second number")] double b)
    {
        var result = a * b;
        return new CalculationResult($"{a} × {b}", result, "Multiplication", DateTime.UtcNow);
    }

    /// <summary>
    /// Divides the first number by the second number.
    /// </summary>
    [McpServerTool, Description("Divides the first number by the second number. Returns an error if dividing by zero.")]
    public static CalculationResult Divide(
        [Description("The dividend (number to be divided)")] double a,
        [Description("The divisor (number to divide by)")] double b)
    {
        if (b == 0)
        {
            return new CalculationResult($"{a} ÷ {b}", double.NaN, "Division Error: Cannot divide by zero", DateTime.UtcNow);
        }
        var result = a / b;
        return new CalculationResult($"{a} ÷ {b}", result, "Division", DateTime.UtcNow);
    }

    /// <summary>
    /// Calculates the power of a number.
    /// </summary>
    [McpServerTool, Description("Raises a base number to an exponent power.")]
    public static CalculationResult Power(
        [Description("The base number")] double baseNumber,
        [Description("The exponent")] double exponent)
    {
        var result = Math.Pow(baseNumber, exponent);
        return new CalculationResult($"{baseNumber} ^ {exponent}", result, "Power", DateTime.UtcNow);
    }

    /// <summary>
    /// Calculates the square root of a number.
    /// </summary>
    [McpServerTool, Description("Calculates the square root of a positive number.")]
    public static CalculationResult SquareRoot(
        [Description("The number to find the square root of (must be non-negative)")] double number)
    {
        if (number < 0)
        {
            return new CalculationResult($"√{number}", double.NaN, "Square Root Error: Cannot calculate square root of negative number", DateTime.UtcNow);
        }
        var result = Math.Sqrt(number);
        return new CalculationResult($"√{number}", result, "Square Root", DateTime.UtcNow);
    }

    /// <summary>
    /// Calculates the percentage of a number.
    /// </summary>
    [McpServerTool, Description("Calculates what percentage one number is of another.")]
    public static CalculationResult Percentage(
        [Description("The partial value")] double part,
        [Description("The whole value")] double whole)
    {
        if (whole == 0)
        {
            return new CalculationResult($"{part} / {whole} × 100", double.NaN, "Percentage Error: Cannot divide by zero", DateTime.UtcNow);
        }
        var result = (part / whole) * 100;
        return new CalculationResult($"({part} / {whole}) × 100", result, "Percentage", DateTime.UtcNow);
    }

    /// <summary>
    /// Calculates the modulo (remainder) of division.
    /// </summary>
    [McpServerTool, Description("Calculates the remainder when dividing two numbers.")]
    public static CalculationResult Modulo(
        [Description("The dividend")] double a,
        [Description("The divisor")] double b)
    {
        if (b == 0)
        {
            return new CalculationResult($"{a} mod {b}", double.NaN, "Modulo Error: Cannot divide by zero", DateTime.UtcNow);
        }
        var result = a % b;
        return new CalculationResult($"{a} mod {b}", result, "Modulo", DateTime.UtcNow);
    }
}

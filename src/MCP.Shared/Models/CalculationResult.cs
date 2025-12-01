namespace MCP.Shared.Models;

/// <summary>
/// Represents the result of a calculation operation.
/// </summary>
public record CalculationResult(
    string Expression,
    double Result,
    string Operation,
    DateTime Timestamp
);

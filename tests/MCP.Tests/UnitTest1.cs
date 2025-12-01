using MCP.Server.Tools;
using MCP.Shared.Models;

namespace MCP.Tests;

public class CalculatorToolTests
{
    [Fact]
    public void Add_ReturnsCorrectSum()
    {
        var result = CalculatorTool.Add(5, 3);

        Assert.Equal(8, result.Result);
        Assert.Equal("Addition", result.Operation);
        Assert.Equal("5 + 3", result.Expression);
    }

    [Fact]
    public void Subtract_ReturnsCorrectDifference()
    {
        var result = CalculatorTool.Subtract(10, 4);

        Assert.Equal(6, result.Result);
        Assert.Equal("Subtraction", result.Operation);
    }

    [Fact]
    public void Multiply_ReturnsCorrectProduct()
    {
        var result = CalculatorTool.Multiply(6, 7);

        Assert.Equal(42, result.Result);
        Assert.Equal("Multiplication", result.Operation);
    }

    [Fact]
    public void Divide_ReturnsCorrectQuotient()
    {
        var result = CalculatorTool.Divide(20, 4);

        Assert.Equal(5, result.Result);
        Assert.Equal("Division", result.Operation);
    }

    [Fact]
    public void Divide_ByZero_ReturnsNaN()
    {
        var result = CalculatorTool.Divide(10, 0);

        Assert.True(double.IsNaN(result.Result));
        Assert.Contains("Cannot divide by zero", result.Operation);
    }

    [Fact]
    public void SquareRoot_ReturnsCorrectResult()
    {
        var result = CalculatorTool.SquareRoot(16);

        Assert.Equal(4, result.Result);
        Assert.Equal("Square Root", result.Operation);
    }

    [Fact]
    public void SquareRoot_NegativeNumber_ReturnsNaN()
    {
        var result = CalculatorTool.SquareRoot(-4);

        Assert.True(double.IsNaN(result.Result));
        Assert.Contains("Cannot calculate square root", result.Operation);
    }

    [Fact]
    public void Power_ReturnsCorrectResult()
    {
        var result = CalculatorTool.Power(2, 8);

        Assert.Equal(256, result.Result);
        Assert.Equal("Power", result.Operation);
    }

    [Fact]
    public void Percentage_ReturnsCorrectResult()
    {
        var result = CalculatorTool.Percentage(25, 100);

        Assert.Equal(25, result.Result);
        Assert.Equal("Percentage", result.Operation);
    }

    [Fact]
    public void Modulo_ReturnsCorrectRemainder()
    {
        var result = CalculatorTool.Modulo(17, 5);

        Assert.Equal(2, result.Result);
        Assert.Equal("Modulo", result.Operation);
    }
}

public class WeatherToolTests
{
    [Fact]
    public void GetWeather_ReturnsValidWeatherInfo()
    {
        var result = WeatherTool.GetWeather("London");

        Assert.NotNull(result);
        Assert.Equal("London", result.Location);
        Assert.Equal("°C", result.Unit);
        Assert.InRange(result.Humidity, 0, 100);
        Assert.InRange(result.WindSpeed, 0, 50);
    }

    [Fact]
    public void GetWeather_WithFahrenheit_ReturnsCorrectUnit()
    {
        var result = WeatherTool.GetWeather("Tokyo", "fahrenheit");

        Assert.NotNull(result);
        Assert.Equal("°F", result.Unit);
    }

    [Fact]
    public void GetForecast_ReturnsCorrectNumberOfDays()
    {
        var result = WeatherTool.GetForecast("Paris", 5);

        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
    }

    [Fact]
    public void GetForecast_ClampsDaysTo7()
    {
        var result = WeatherTool.GetForecast("Sydney", 10);

        Assert.NotNull(result);
        Assert.Equal(7, result.Count);
    }
}

public class TodoToolTests
{
    [Fact]
    public void CreateTodo_CreatesNewTodoItem()
    {
        var result = TodoTool.CreateTodo("Test Todo", "Test Description", "High");

        Assert.NotNull(result);
        Assert.Equal("Test Todo", result.Title);
        Assert.Equal("Test Description", result.Description);
        Assert.Equal("High", result.Priority);
        Assert.False(result.IsCompleted);
    }

    [Fact]
    public void CreateTodo_DefaultsToMediumPriority()
    {
        var result = TodoTool.CreateTodo("Simple Todo");

        Assert.Equal("Medium", result.Priority);
    }

    [Fact]
    public void CreateTodo_InvalidPriority_DefaultsToMedium()
    {
        var result = TodoTool.CreateTodo("Todo", "", "Invalid");

        Assert.Equal("Medium", result.Priority);
    }

    [Fact]
    public void GetTodoStats_ReturnsValidStats()
    {
        // Create some todos for testing
        TodoTool.CreateTodo("Stat Test 1", "", "High");
        TodoTool.CreateTodo("Stat Test 2", "", "Low");

        var stats = TodoTool.GetTodoStats();

        Assert.NotNull(stats);
    }

    [Fact]
    public void CompleteTodo_MarksAsCompleted()
    {
        var todo = TodoTool.CreateTodo("Complete Me");
        var completed = TodoTool.CompleteTodo(todo.Id);

        Assert.NotNull(completed);
        Assert.True(completed.IsCompleted);
        Assert.NotNull(completed.CompletedAt);
    }

    [Fact]
    public void DeleteTodo_RemovesTodo()
    {
        var todo = TodoTool.CreateTodo("Delete Me");
        var deleted = TodoTool.DeleteTodo(todo.Id);

        Assert.True(deleted);
    }

    [Fact]
    public void DeleteTodo_NonExistent_ReturnsFalse()
    {
        var deleted = TodoTool.DeleteTodo("non-existent-id");

        Assert.False(deleted);
    }
}

public class TextUtilityToolTests
{
    [Fact]
    public void AnalyzeText_ReturnsCorrectCounts()
    {
        var text = "Hello World!";
        var result = TextUtilityTool.AnalyzeText(text);
        
        // Use reflection since the result is an anonymous type
        var resultType = result.GetType();
        var wordsProperty = resultType.GetProperty("Words");
        var charsProperty = resultType.GetProperty("Characters");
        
        Assert.NotNull(wordsProperty);
        Assert.NotNull(charsProperty);
        Assert.Equal(2, wordsProperty.GetValue(result));
        Assert.Equal(12, charsProperty.GetValue(result));
    }

    [Fact]
    public void ConvertCase_Upper_ConvertsCorrectly()
    {
        var result = TextUtilityTool.ConvertCase("hello world", "upper");

        Assert.Equal("HELLO WORLD", result);
    }

    [Fact]
    public void ConvertCase_Lower_ConvertsCorrectly()
    {
        var result = TextUtilityTool.ConvertCase("HELLO WORLD", "lower");

        Assert.Equal("hello world", result);
    }

    [Fact]
    public void ConvertCase_Title_ConvertsCorrectly()
    {
        var result = TextUtilityTool.ConvertCase("hello world", "title");

        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void ReverseText_ReversesCorrectly()
    {
        var result = TextUtilityTool.ReverseText("hello");

        Assert.Equal("olleh", result);
    }

    [Fact]
    public void Slugify_CreatesValidSlug()
    {
        var result = TextUtilityTool.Slugify("Hello World! This is a Test");

        Assert.Equal("hello-world-this-is-a-test", result);
    }

    [Fact]
    public void ExtractEmails_FindsEmails()
    {
        var text = "Contact us at test@example.com or support@company.org";
        var result = TextUtilityTool.ExtractEmails(text);

        Assert.Contains("test@example.com", result);
        Assert.Contains("support@company.org", result);
    }

    [Fact]
    public void ExtractUrls_FindsUrls()
    {
        var text = "Visit https://example.com or http://test.org for more info";
        var result = TextUtilityTool.ExtractUrls(text);

        Assert.Contains("https://example.com", result);
        Assert.Contains("http://test.org", result);
    }

    [Fact]
    public void Truncate_TruncatesCorrectly()
    {
        var result = TextUtilityTool.Truncate("This is a long text that needs truncation", 20);

        Assert.Equal(20, result.Length);
        Assert.EndsWith("...", result);
    }

    [Fact]
    public void RemoveDuplicateLines_RemovesDuplicates()
    {
        var text = "line1\nline2\nline1\nline3\nline2";
        var result = TextUtilityTool.RemoveDuplicateLines(text);

        Assert.Equal("line1\nline2\nline3", result);
    }
}

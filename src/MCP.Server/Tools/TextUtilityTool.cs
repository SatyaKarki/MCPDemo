using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using ModelContextProtocol.Server;

namespace MCP.Server.Tools;

/// <summary>
/// MCP Tool providing text utility operations like formatting, analysis, and transformation.
/// </summary>
[McpServerToolType]
public static partial class TextUtilityTool
{
    /// <summary>
    /// Counts the number of words, characters, and lines in the given text.
    /// </summary>
    [McpServerTool, Description("Analyzes text and returns word count, character count (with and without spaces), and line count.")]
    public static object AnalyzeText(
        [Description("The text to analyze")] string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return new
            {
                Words = 0,
                Characters = 0,
                CharactersWithoutSpaces = 0,
                Lines = 0,
                Sentences = 0,
                Paragraphs = 0
            };
        }

        var words = text.Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries).Length;
        var characters = text.Length;
        var charactersWithoutSpaces = text.Replace(" ", "").Replace("\t", "").Replace("\n", "").Replace("\r", "").Length;
        var lines = text.Split('\n').Length;
        var sentences = SentenceRegex().Matches(text).Count;
        var paragraphs = text.Split(["\n\n", "\r\n\r\n"], StringSplitOptions.RemoveEmptyEntries).Length;

        return new
        {
            Words = words,
            Characters = characters,
            CharactersWithoutSpaces = charactersWithoutSpaces,
            Lines = lines,
            Sentences = sentences,
            Paragraphs = paragraphs
        };
    }

    /// <summary>
    /// Converts text to different cases.
    /// </summary>
    [McpServerTool, Description("Converts text to different cases: upper, lower, title, sentence, or toggle.")]
    public static string ConvertCase(
        [Description("The text to convert")] string text,
        [Description("Case type: 'upper', 'lower', 'title', 'sentence', or 'toggle'")] string caseType)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return caseType.ToLowerInvariant() switch
        {
            "upper" => text.ToUpperInvariant(),
            "lower" => text.ToLowerInvariant(),
            "title" => ToTitleCase(text),
            "sentence" => ToSentenceCase(text),
            "toggle" => ToggleCase(text),
            _ => text
        };
    }

    /// <summary>
    /// Reverses the given text.
    /// </summary>
    [McpServerTool, Description("Reverses the characters in the given text.")]
    public static string ReverseText(
        [Description("The text to reverse")] string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var charArray = text.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }

    /// <summary>
    /// Removes duplicate lines from text.
    /// </summary>
    [McpServerTool, Description("Removes duplicate lines from the text, keeping only unique lines.")]
    public static string RemoveDuplicateLines(
        [Description("The text with potentially duplicate lines")] string text,
        [Description("Whether comparison should be case-sensitive. Defaults to true.")] bool caseSensitive = true)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var lines = text.Split('\n');
        var uniqueLines = caseSensitive
            ? lines.Distinct()
            : lines.Distinct(StringComparer.OrdinalIgnoreCase);

        return string.Join('\n', uniqueLines);
    }

    /// <summary>
    /// Extracts all email addresses from text.
    /// </summary>
    [McpServerTool, Description("Extracts all email addresses found in the given text.")]
    public static List<string> ExtractEmails(
        [Description("The text to search for email addresses")] string text)
    {
        if (string.IsNullOrEmpty(text))
            return [];

        var matches = EmailRegex().Matches(text);
        return [.. matches.Cast<Match>().Select(m => m.Value).Distinct()];
    }

    /// <summary>
    /// Extracts all URLs from text.
    /// </summary>
    [McpServerTool, Description("Extracts all URLs found in the given text.")]
    public static List<string> ExtractUrls(
        [Description("The text to search for URLs")] string text)
    {
        if (string.IsNullOrEmpty(text))
            return [];

        var matches = UrlRegex().Matches(text);
        return [.. matches.Cast<Match>().Select(m => m.Value).Distinct()];
    }

    /// <summary>
    /// Slugifies text for URL-friendly strings.
    /// </summary>
    [McpServerTool, Description("Converts text to a URL-friendly slug (lowercase, hyphens instead of spaces, no special characters).")]
    public static string Slugify(
        [Description("The text to convert to a slug")] string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // Convert to lowercase
        var slug = text.ToLowerInvariant();
        // Replace spaces with hyphens
        slug = slug.Replace(' ', '-');
        // Remove special characters
        slug = SlugifyRegex().Replace(slug, "");
        // Remove multiple consecutive hyphens
        slug = MultipleHyphensRegex().Replace(slug, "-");
        // Trim hyphens from start and end
        slug = slug.Trim('-');

        return slug;
    }

    /// <summary>
    /// Truncates text to a specified length.
    /// </summary>
    [McpServerTool, Description("Truncates text to a specified maximum length, optionally adding an ellipsis.")]
    public static string Truncate(
        [Description("The text to truncate")] string text,
        [Description("Maximum length of the result")] int maxLength,
        [Description("Whether to add '...' at the end. Defaults to true.")] bool addEllipsis = true)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;

        var truncated = text[..(addEllipsis ? maxLength - 3 : maxLength)].TrimEnd();
        return addEllipsis ? truncated + "..." : truncated;
    }

    private static string ToTitleCase(string text)
    {
        var words = text.Split(' ');
        var result = new StringBuilder();

        foreach (var word in words)
        {
            if (word.Length > 0)
            {
                result.Append(char.ToUpperInvariant(word[0]));
                if (word.Length > 1)
                    result.Append(word[1..].ToLowerInvariant());
                result.Append(' ');
            }
        }

        return result.ToString().TrimEnd();
    }

    private static string ToSentenceCase(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var result = new StringBuilder(text.ToLowerInvariant());
        var capitalizeNext = true;

        for (int i = 0; i < result.Length; i++)
        {
            if (capitalizeNext && char.IsLetter(result[i]))
            {
                result[i] = char.ToUpperInvariant(result[i]);
                capitalizeNext = false;
            }
            else if (result[i] == '.' || result[i] == '!' || result[i] == '?')
            {
                capitalizeNext = true;
            }
        }

        return result.ToString();
    }

    private static string ToggleCase(string text)
    {
        var result = new StringBuilder();
        foreach (var c in text)
        {
            result.Append(char.IsUpper(c) ? char.ToLowerInvariant(c) : char.ToUpperInvariant(c));
        }
        return result.ToString();
    }

    [GeneratedRegex(@"[.!?]+")]
    private static partial Regex SentenceRegex();

    [GeneratedRegex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}")]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"https?://[^\s<>""']+")]
    private static partial Regex UrlRegex();

    [GeneratedRegex(@"[^a-z0-9\-]")]
    private static partial Regex SlugifyRegex();

    [GeneratedRegex(@"-+")]
    private static partial Regex MultipleHyphensRegex();
}

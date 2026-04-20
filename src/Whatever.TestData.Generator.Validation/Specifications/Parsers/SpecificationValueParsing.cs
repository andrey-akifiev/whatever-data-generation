using Whatever.TestData.Generator.Validation.Specifications.Exceptions;

namespace Whatever.TestData.Generator.Validation.Specifications.Parsers;

/// <summary>
/// Helpers for interpreting textual flags in specification columns.
/// </summary>
public static class SpecificationValueParsing
{
    /// <summary>
    /// Parses requiredness from Y/N/O/R, integers, or booleans.
    /// </summary>
    public static bool ParseRequired(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        raw = raw.Trim();
        if (bool.TryParse(raw, out bool b))
            return b;

        if (int.TryParse(raw, out int i))
            return i == 1;

        return raw.ToUpperInvariant() switch
        {
            "Y" or "YES" or "R" or "REQUIRED" => true,
            "N" or "NO" or "O" or "OPTIONAL" => false,
            _ => throw new SpecificationParseException($"Unknown required flag '{raw}'."),
        };
    }

    /// <summary>
    /// Parses NULL permission from Y/N, integers, or booleans. Empty means not nullable.
    /// </summary>
    public static bool ParseAllowsNull(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        raw = raw.Trim();
        if (bool.TryParse(raw, out bool b))
            return b;

        if (int.TryParse(raw, out int i))
            return i == 1;

        return raw.ToUpperInvariant() switch
        {
            "Y" or "YES" => true,
            "N" or "NO" => false,
            _ => throw new SpecificationParseException($"Unknown NULL flag '{raw}'."),
        };
    }
}
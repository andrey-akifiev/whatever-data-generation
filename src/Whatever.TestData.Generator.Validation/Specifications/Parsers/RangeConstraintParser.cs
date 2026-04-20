using System.Globalization;

using Whatever.TestData.Generator.Validation.Abstractions;
using Whatever.TestData.Generator.Validation.Specifications.Exceptions;

namespace Whatever.TestData.Generator.Validation.Specifications.Parsers;

/// <summary>
/// Parses interval and discrete-set syntax from the <c>Range</c> column.
/// </summary>
public static class RangeConstraintParser
{
    /// <summary>
    /// Parses a range expression for the given logical SQL type.
    /// </summary>
    public static RangeConstraint Parse(string? expression, DataType type)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return new RangeConstraint(false, null, null, null);
        }

        string trimmed = expression.Trim();
        List<string> segments = SplitTopLevel(trimmed);

        bool nullListed = false;
        NumericInterval? numeric = null;
        NumericInterval? strLen = null;
        List<string>? discreteList = null;

        foreach (string segment in segments)
        {
            string s = segment.Trim();
            if (s.Length >= 2 && s[0] == '{' && s[^1] == '}')
            {
                AppendDiscreteTokensFromBraces(s, ref nullListed, ref discreteList);
                continue;
            }

            if (LooksLikeInterval(s))
            {
                NumericInterval interval = ParseInterval(s);
                if (type.Kind == DataTypeKind.String)
                {
                    strLen = interval;
                }
                else
                {
                    numeric = interval;
                }
            }
        }

        IReadOnlyList<string>? discrete =
            discreteList is { Count: > 0 } ? discreteList : null;

        return new RangeConstraint(nullListed, numeric, strLen, discrete);
    }

    private static bool LooksLikeInterval(string s)
    {
        return s.Length >= 2 && (s[0] == '[' || s[0] == '(') && (s[^1] == ']' || s[^1] == ')');
    }

    private static void AppendDiscreteTokensFromBraces(
        string s,
        ref bool nullListed,
        ref List<string>? discreteList)
    {
        string inner = s[1..^1];
        foreach (string token in inner.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            if (token.Equals("NULL", StringComparison.OrdinalIgnoreCase))
            {
                nullListed = true;
                continue;
            }

            discreteList ??= new List<string>();
            discreteList.Add(token);
        }
    }

    private static List<string> SplitTopLevel(string input)
    {
        List<string> result = new ();
        int depthSquare = 0;
        int depthRound = 0;
        int depthCurly = 0;
        int start = 0;
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            switch (c)
            {
                case '[':
                    depthSquare++;
                    break;
                case ']':
                    depthSquare--;
                    break;
                case '(':
                    depthRound++;
                    break;
                case ')':
                    depthRound--;
                    break;
                case '{':
                    depthCurly++;
                    break;
                case '}':
                    depthCurly--;
                    break;
                case ',' when depthSquare == 0 && depthRound == 0 && depthCurly == 0:
                    result.Add(input[start..i]);
                    start = i + 1;
                    break;
            }
        }

        result.Add(input[start..]);
        return result;
    }

    private static NumericInterval ParseInterval(string s)
    {
        s = s.Trim();
        char leftBracket = s[0];
        char rightBracket = s[^1];
        string inner = s[1..^1];
        int comma = FindTopLevelComma(inner);
        if (comma < 0)
        {
            throw new SpecificationParseException($"Interval must contain a comma: '{s}'.");
        }

        string leftToken = inner[..comma].Trim();
        string rightToken = inner[(comma + 1)..].Trim();

        bool leftInclusive = leftBracket == '[';
        bool rightInclusive = rightBracket == ']';

        NumericEndpoint min = ParseEndpoint(leftToken, isUnboundedWhenEmpty: true, inclusive: leftInclusive);
        NumericEndpoint max = ParseEndpoint(rightToken, isUnboundedWhenEmpty: true, inclusive: rightInclusive);
        return new NumericInterval(min, max);
    }

    private static int FindTopLevelComma(string inner)
    {
        int depthSquare = 0;
        for (int i = 0; i < inner.Length; i++)
        {
            switch (inner[i])
            {
                case '[':
                    depthSquare++;
                    break;
                case ']':
                    depthSquare--;
                    break;
                case ',' when depthSquare == 0:
                    return i;
            }
        }

        return -1;
    }

    private static NumericEndpoint ParseEndpoint(string token, bool isUnboundedWhenEmpty, bool inclusive)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            if (!isUnboundedWhenEmpty)
            {
                throw new SpecificationParseException("Empty bound is not allowed here.");
            }

            return new NumericEndpoint(true, 0, inclusive);
        }

        if (!decimal.TryParse(token, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value))
        {
            throw new SpecificationParseException($"Invalid numeric bound '{token}'.");
        }

        return new NumericEndpoint(false, value, inclusive);
    }
}
using System.Globalization;
using System.Text.RegularExpressions;

using Whatever.TestData.Generator.Validation.Abstractions;
using Whatever.TestData.Generator.Validation.Specifications.Exceptions;

namespace Whatever.TestData.Generator.Validation.Specifications.Parsers;

/// <summary>
/// Parses SQL-like type strings from specifications into <see cref="DataType"/>.
/// </summary>
public static class DataTypeParser
{
    private static readonly Regex TypeRegex = new(
        @"^\s*(?<name>[a-zA-Z]+)\s*(?:\(\s*(?<a>-?\d+)\s*(?:,\s*(?<b>-?\d+)\s*)?\))?\s*$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    /// <summary>
    /// Parses a type string such as <c>NVARCHAR(100)</c> or <c>INT</c>.
    /// </summary>
    /// <param name="rawType">Raw type text from the specification.</param>
    /// <returns>Parsed type information.</returns>
    /// <exception cref="SpecificationParseException">When the type is not recognized.</exception>
    public static DataType Parse(string rawType)
    {
        if (string.IsNullOrWhiteSpace(rawType))
        {
            throw new SpecificationParseException("Type column is empty.");
        }

        Match match = TypeRegex.Match(rawType.Trim());
        if (!match.Success)
        {
            throw new SpecificationParseException($"Unrecognized type format: '{rawType}'.");
        }

        string name = match.Groups["name"].Value.ToLowerInvariant();
        int? a = match.Groups["a"].Success
            ? int.Parse(match.Groups["a"].Value, CultureInfo.InvariantCulture)
            : null;
        int? b = match.Groups["b"].Success
            ? int.Parse(match.Groups["b"].Value, CultureInfo.InvariantCulture)
            : null;

        return name switch
        {
            "nvarchar" or "varchar" or "nchar" or "char" or "text" or "ntext" or "string" =>
                new DataType(DataTypeKind.String, maxLength: a ?? int.MaxValue),

            "int" or "integer" => new DataType(DataTypeKind.Integral),
            "bigint" => new DataType(DataTypeKind.Integral),
            "smallint" => new DataType(DataTypeKind.Integral),
            "tinyint" => new DataType(DataTypeKind.Integral),

            "float" or "real" => new DataType(DataTypeKind.Floating),
            "decimal" or "numeric" => new DataType(DataTypeKind.Floating, precision: a, scale: b),

            "bit" => new DataType(DataTypeKind.Boolean),

            "datetime" or "datetime2" or "smalldatetime" or "date" or "time" =>
                new DataType(DataTypeKind.DateTime),

            "uniqueidentifier" or "guid" or "uuid" => new DataType(DataTypeKind.Guid),

            _ => throw new SpecificationParseException($"Unsupported SQL type: '{rawType}'."),
        };
    }
}
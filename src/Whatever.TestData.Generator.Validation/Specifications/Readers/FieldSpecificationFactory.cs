using Whatever.TestData.Generator.Validation.Abstractions;
using Whatever.TestData.Generator.Validation.Specifications.Exceptions;
using Whatever.TestData.Generator.Validation.Specifications.Parsers;

namespace Whatever.TestData.Generator.Validation.Specifications.Readers;

/// <summary>
/// Builds <see cref="FieldSpecification"/> instances from raw row values.
/// </summary>
public static class FieldSpecificationFactory
{
    /// <summary>
    /// Creates a field specification from textual columns.
    /// </summary>
    public static FieldSpecification Create(
        string name,
        string? requiredRaw,
        string typeRaw,
        string? rangeRaw,
        string? defaultValue,
        string? nullRaw)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new SpecificationParseException("Field name is empty.");
        }

        bool isRequired = SpecificationValueParsing.ParseRequired(requiredRaw);
        DataType dataType = DataTypeParser.Parse(typeRaw);
        bool allowsNull = SpecificationValueParsing.ParseAllowsNull(nullRaw);
        RangeConstraint range = RangeConstraintParser.Parse(rangeRaw, dataType);

        return new FieldSpecification(
            name.Trim(),
            isRequired,
            dataType,
            rangeRaw,
            defaultValue,
            allowsNull,
            range);
    }
}
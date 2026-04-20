using Whatever.TestData.Generator.Validation.Abstractions;

namespace Whatever.TestData.Generator.Validation.Scenarios;

/// <summary>
/// Builds per-file dictionaries of cell values for a concrete scenario.
/// </summary>
public static class ScenarioMatrixBuilder
{
    /// <summary>
    /// Creates a matrix of values keyed by logical file name and then field name.
    /// </summary>
    public static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string?>> Build(
        IReadOnlyList<FileSpecification> specifications,
        TestScenarioDescriptor scenario)
    {
        Dictionary<string, IReadOnlyDictionary<string, string?>> matrix = new (StringComparer.OrdinalIgnoreCase);

        foreach (FileSpecification spec in specifications)
        {
            Dictionary<string, string?> row = new (StringComparer.OrdinalIgnoreCase);
            foreach (FieldSpecification field in spec.Fields)
            {
                if (ShouldIncludeField(spec, field, scenario))
                {
                    row[field.Name] = ResolveValue(spec, field, scenario);
                }
            }

            matrix[spec.LogicalName] = row;
        }

        return matrix;
    }

    private static bool ShouldIncludeField(FileSpecification spec, FieldSpecification field, TestScenarioDescriptor scenario)
    {
        bool isTarget = IsTarget(spec, field, scenario);
        if (isTarget)
            return scenario.BodyKind != ScenarioBodyKind.MissedField;

        return field.IsRequired;
    }

    private static bool IsTarget(FileSpecification spec, FieldSpecification field, TestScenarioDescriptor scenario) =>
        spec.LogicalName.Equals(scenario.TargetLogicalFile, StringComparison.OrdinalIgnoreCase)
        && field.Name.Equals(scenario.TargetField, StringComparison.OrdinalIgnoreCase);

    private static string? ResolveValue(FileSpecification spec, FieldSpecification field, TestScenarioDescriptor scenario)
    {
        if (IsTarget(spec, field, scenario))
            return MaterialiseTarget(scenario);

        return SampleValid(field);
    }

    private static string? MaterialiseTarget(TestScenarioDescriptor scenario) =>
        scenario.BodyKind switch
        {
            ScenarioBodyKind.NullLiteral => null,
            ScenarioBodyKind.EmptyString => string.Empty,
            ScenarioBodyKind.SingleSpace => " ",
            ScenarioBodyKind.Tab => "\t",
            ScenarioBodyKind.Newline => "\n",
            ScenarioBodyKind.Literal => scenario.Literal,
            ScenarioBodyKind.MissedField => throw new InvalidOperationException("Missed fields must be excluded from the row."),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
        };

    private static string SampleValid(FieldSpecification field) =>
        field.DataType.Kind switch
        {
            DataTypeKind.String => ValidValueSampler.SampleValidString(field),
            DataTypeKind.Integral => ValidValueSampler.SampleValidIntegral(field),
            DataTypeKind.Floating => ValidValueSampler.SampleValidFloating(field),
            DataTypeKind.Boolean => ValidValueSampler.SampleValidBoolean(field),
            DataTypeKind.DateTime => ValidValueSampler.SampleValidDateTime(field),
            DataTypeKind.Guid => ValidValueSampler.SampleValidGuid(field),
            _ => string.Empty,
        };
}
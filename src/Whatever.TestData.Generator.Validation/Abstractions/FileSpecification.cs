namespace Whatever.TestData.Generator.Validation.Abstractions;

/// <summary>
/// Logical file specification (for example one CSV dataset or one JSON object schema).
/// </summary>
public sealed class FileSpecification(string logicalName, IReadOnlyList<FieldSpecification> fields)
{
    /// <summary>
    /// Stable name used for output files (accounts, locations, ...).
    /// </summary>
    public string LogicalName { get; } = logicalName;

    /// <summary>
    /// List of fields (columns or JSON properties) that should be present in the generated dataset according to the specification.
    /// </summary>
    public IReadOnlyList<FieldSpecification> Fields { get; } = fields;
}
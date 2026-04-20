namespace Whatever.TestData.Generator.Validation.Specifications.Readers;

/// <summary>
/// Options that influence how specification files are interpreted.
/// </summary>
public sealed class SpecificationReaderOptions
{
    /// <summary>
    /// For XLSX inputs, optional case-insensitive sheet names to import. When empty, all sheets are read as separate specifications.
    /// </summary>
    public IReadOnlyList<string>? SheetFilter { get; init; }

    /// <summary>
    /// Logical-to-physical column/property mapping used by readers.
    /// </summary>
    public SpecificationColumnMapping ColumnMapping { get; init; } = SpecificationColumnMapping.Default;
}
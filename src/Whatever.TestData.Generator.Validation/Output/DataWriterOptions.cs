namespace Whatever.TestData.Generator.Validation.Output;

/// <summary>
/// Cross-cutting options for writing generated files.
/// </summary>
public sealed class DataWriterOptions
{
    /// <summary>Optional prefix applied to every file name, separated by an underscore.</summary>
    public string? FilePrefix { get; init; }
}
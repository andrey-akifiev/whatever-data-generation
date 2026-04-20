using Whatever.TestData.Generator.Validation.Abstractions;

namespace Whatever.TestData.Generator.Validation.Output;

/// <summary>
/// Writes a single-row dataset for a logical specification.
/// </summary>
public interface IFileDatasetWriter
{
    /// <summary>
    /// The physical format emitted by this writer.
    /// </summary>
    GeneratedDataFormat Format { get; }

    /// <summary>
    /// Writes a dataset to <paramref name="filePath"/>. Only fields present in <paramref name="row"/> are emitted.
    /// </summary>
    Task WriteAsync(
        string filePath,
        FileSpecification specification,
        IReadOnlyDictionary<string, string?> row,
        CancellationToken cancellationToken = default);
}
namespace Whatever.TestData.Generator.Validation.Output;

/// <summary>
/// Creates fresh <see cref="IFileDatasetWriter"/> instances. Implementations must be safe for concurrent calls.
/// </summary>
public interface IDatasetWriterFactory
{
    /// <summary>
    /// Returns a new writer instance for the requested format.
    /// </summary>
    IFileDatasetWriter CreateWriter(GeneratedDataFormat format);
}
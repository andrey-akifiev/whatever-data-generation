using Whatever.TestData.Generator.Validation.Abstractions;

namespace Whatever.TestData.Generator.Validation.Specifications.Readers;

/// <summary>
/// Reads a <see cref="FileSpecification"/> from a physical specification file.
/// </summary>
public interface ISpecificationReader
{
    /// <summary>
    /// Extensions this reader supports, including the leading dot.
    /// </summary>
    IReadOnlyCollection<string> SupportedExtensions { get; }

    /// <summary>
    /// Reads a specification from disk. The logical name is derived from the file or sheet name.
    /// </summary>
    Task<FileSpecification> ReadFileAsync(
        string filePath,
        SpecificationReaderOptions options,
        CancellationToken cancellationToken = default);
}
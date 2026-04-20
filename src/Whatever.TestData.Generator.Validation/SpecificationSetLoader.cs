using Microsoft.Extensions.Logging;

using Whatever.TestData.Generator.Validation.Abstractions;
using Whatever.TestData.Generator.Validation.Specifications.Readers;

namespace Whatever.TestData.Generator.Validation;

/// <summary>
/// Loads one or more <see cref="FileSpecification"/> instances from mixed specification sources.
/// </summary>
public sealed class SpecificationSetLoader(ILogger<SpecificationSetLoader>? logger)
{
    /// <summary>
    /// Loads all specifications referenced by <paramref name="specificationPaths"/>.
    /// </summary>
    public async Task<IReadOnlyList<FileSpecification>> LoadAsync(
        IReadOnlyList<string> specificationPaths,
        SpecificationReaderOptions readerOptions,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specificationPaths);
        if (specificationPaths.Count == 0)
        {
            throw new ArgumentException("At least one specification path is required.", nameof(specificationPaths));
        }

        List<FileSpecification> results = [];
        foreach (string path in specificationPaths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Specification file not found.", path);
            }

            string extension = Path.GetExtension(path);
            logger?.LogInformation("Loading specification {Path}", path);

            if (extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                XlsxSpecificationReader reader = new XlsxSpecificationReader();
                results.AddRange(reader.ReadWorkbook(path, readerOptions));
                continue;
            }

            if (extension.Equals(".json", StringComparison.OrdinalIgnoreCase))
            {
                string text = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
                if (text.TrimStart().StartsWith('['))
                {
                    results.AddRange(JsonSpecificationReader.ReadMany(path, readerOptions.ColumnMapping, cancellationToken));
                }
                else
                {
                    JsonSpecificationReader reader = new JsonSpecificationReader();
                    results.Add(await reader.ReadFileAsync(path, readerOptions, cancellationToken).ConfigureAwait(false));
                }

                continue;
            }

            if (extension.Equals(".csv", StringComparison.OrdinalIgnoreCase))
            {
                CsvSpecificationReader reader = new CsvSpecificationReader();
                results.Add(await reader.ReadFileAsync(path, readerOptions, cancellationToken).ConfigureAwait(false));
                continue;
            }

            throw new NotSupportedException($"Unsupported specification extension '{extension}' for '{path}'.");
        }

        return results;
    }
}
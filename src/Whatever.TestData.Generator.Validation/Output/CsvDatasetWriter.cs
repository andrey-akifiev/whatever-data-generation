using System.Globalization;

using CsvHelper;
using CsvHelper.Configuration;

using Microsoft.Extensions.Logging;

using Whatever.TestData.Generator.Validation.Abstractions;

namespace Whatever.TestData.Generator.Validation.Output;

/// <summary>
/// Writes a single CSV row following the declaration order in the specification.
/// </summary>
public sealed class CsvDatasetWriter(ILogger<CsvDatasetWriter>? logger)
    : IFileDatasetWriter
{
    /// <inheritdoc />
    public GeneratedDataFormat Format => GeneratedDataFormat.Csv;

    /// <inheritdoc />
    public async Task WriteAsync(
        string filePath,
        FileSpecification specification,
        IReadOnlyDictionary<string, string?> row,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);
        ArgumentNullException.ThrowIfNull(row);

        await using FileStream stream = File.Create(filePath);
        await using StreamWriter writer = new StreamWriter(stream);
        await using CsvWriter csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
        });

        List<string> columns = specification.Fields.Where(f => row.ContainsKey(f.Name)).Select(f => f.Name).ToList();
        foreach (string column in columns)
        {
            csv.WriteField(column);
        }

        await csv.NextRecordAsync().ConfigureAwait(false);

        foreach (string column in columns)
        {
            row.TryGetValue(column, out string? value);
            csv.WriteField(value);
        }

        await csv.NextRecordAsync().ConfigureAwait(false);
        logger?.LogDebug("Wrote CSV dataset to {Path}", filePath);
    }
}
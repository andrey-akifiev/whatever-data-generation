using Microsoft.Extensions.Logging;

using Parquet;
using Parquet.Data;
using Parquet.Schema;

using Whatever.TestData.Generator.Validation.Abstractions;

namespace Whatever.TestData.Generator.Validation.Output;

/// <summary>
/// Writes a single-row Parquet file using string-typed columns for maximum compatibility with heterogeneous test values.
/// </summary>
public sealed class ParquetDatasetWriter(ILogger<ParquetDatasetWriter>? logger) : IFileDatasetWriter
{
    /// <inheritdoc />
    public GeneratedDataFormat Format => GeneratedDataFormat.Parquet;

    /// <inheritdoc />
    public async Task WriteAsync(
        string filePath,
        FileSpecification specification,
        IReadOnlyDictionary<string, string?> row,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);
        ArgumentNullException.ThrowIfNull(row);

        List<string> columns = specification.Fields.Where(f => row.ContainsKey(f.Name)).Select(f => f.Name).ToList();
        Field[] fields = columns.Select(c => new DataField<string>(c)).ToArray<Field>();
        ParquetSchema schema = new ParquetSchema(fields);

        await using FileStream stream = File.Create(filePath);
        using ParquetWriter writer = await ParquetWriter.CreateAsync(schema, stream, cancellationToken: cancellationToken).ConfigureAwait(false);
        using ParquetRowGroupWriter group = writer.CreateRowGroup();

        for (int i = 0; i < columns.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            row.TryGetValue(columns[i], out string? value);
            DataColumn column = new DataColumn((DataField<string>)schema.Fields[i], new[] { value });
            await group.WriteColumnAsync(column, cancellationToken).ConfigureAwait(false);
        }

        logger?.LogDebug("Wrote Parquet dataset to {Path}", filePath);
    }
}
using System.Text.Json;
using System.Text.Json.Nodes;

using Microsoft.Extensions.Logging;

using Whatever.TestData.Generator.Validation.Abstractions;

namespace Whatever.TestData.Generator.Validation.Output;

/// <summary>
/// Writes a single JSON object containing the generated row.
/// </summary>
public sealed class JsonDatasetWriter(ILogger<JsonDatasetWriter>? logger) : IFileDatasetWriter
{
    /// <inheritdoc />
    public GeneratedDataFormat Format => GeneratedDataFormat.Json;

    /// <inheritdoc />
    public async Task WriteAsync(
        string filePath,
        FileSpecification specification,
        IReadOnlyDictionary<string, string?> row,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);
        ArgumentNullException.ThrowIfNull(row);

        JsonObject node = new JsonObject();
        foreach (FieldSpecification field in specification.Fields)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (row.TryGetValue(field.Name, out string? value))
            {
                node[field.Name] = value is null ? null : JsonValue.Create(value);
            }
        }

        JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
        await File.WriteAllTextAsync(filePath, node.ToJsonString(options), cancellationToken).ConfigureAwait(false);
        logger?.LogDebug("Wrote JSON dataset to {Path}", filePath);
    }
}
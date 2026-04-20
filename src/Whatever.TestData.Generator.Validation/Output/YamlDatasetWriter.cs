using Microsoft.Extensions.Logging;

using Whatever.TestData.Generator.Validation.Abstractions;

using YamlDotNet.RepresentationModel;

namespace Whatever.TestData.Generator.Validation.Output;

/// <summary>
/// Writes a single YAML mapping for the generated row.
/// </summary>
public sealed class YamlDatasetWriter(ILogger<YamlDatasetWriter>? logger) : IFileDatasetWriter
{
    /// <inheritdoc />
    public GeneratedDataFormat Format => GeneratedDataFormat.Yaml;

    /// <inheritdoc />
    public async Task WriteAsync(
        string filePath,
        FileSpecification specification,
        IReadOnlyDictionary<string, string?> row,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);
        ArgumentNullException.ThrowIfNull(row);

        YamlMappingNode root = new YamlMappingNode();
        foreach (FieldSpecification field in specification.Fields)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (row.TryGetValue(field.Name, out string? value))
            {
                YamlNode node = value is null
                    ? new YamlScalarNode("~")
                    : new YamlScalarNode(value);

                root.Add(field.Name, node);
            }
        }

        YamlDocument document = new (root);
        await using StringWriter writer = new ();
        YamlStream yamlStream = new (document);
        yamlStream.Save(writer, assignAnchors: false);

        await File.WriteAllTextAsync(filePath, writer.ToString(), cancellationToken).ConfigureAwait(false);
        logger?.LogDebug("Wrote YAML dataset to {Path}", filePath);
    }
}
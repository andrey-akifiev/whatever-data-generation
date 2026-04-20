using System.Text.Json;

using Whatever.TestData.Generator.Validation.Abstractions;
using Whatever.TestData.Generator.Validation.Specifications.Exceptions;

namespace Whatever.TestData.Generator.Validation.Specifications.Readers;

/// <summary>
/// Reads specifications from JSON files. Supports either a single object or an array of objects.
/// </summary>
public sealed class JsonSpecificationReader : ISpecificationReader
{
    private static readonly string[] Extensions = [".json"];

    /// <inheritdoc />
    public IReadOnlyCollection<string> SupportedExtensions => Extensions;

    /// <inheritdoc />
    public async Task<FileSpecification> ReadFileAsync(
        string filePath,
        SpecificationReaderOptions options,
        CancellationToken cancellationToken = default)
    {
        await using FileStream stream = File.OpenRead(filePath);
        SpecificationColumnMapping mapping = options.ColumnMapping;
        JsonDocument root = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
        return root.RootElement.ValueKind switch
        {
            JsonValueKind.Object => ParseSingle(root.RootElement, Path.GetFileNameWithoutExtension(filePath), mapping),
            JsonValueKind.Array when root.RootElement.GetArrayLength() > 0 =>
                ParseSingle(root.RootElement[0], Path.GetFileNameWithoutExtension(filePath), mapping),
            _ => throw new SpecificationParseException("JSON root must be an object or a non-empty array."),
        };
    }

    /// <summary>
    /// Reads all file objects from a JSON array root.
    /// </summary>
    public static IReadOnlyList<FileSpecification> ReadMany(
        string filePath,
        SpecificationColumnMapping? mapping = null,
        CancellationToken cancellationToken = default)
    {
        mapping ??= SpecificationColumnMapping.Default;
        using FileStream stream = File.OpenRead(filePath);
        using JsonDocument root = JsonDocument.Parse(stream, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });
        if (root.RootElement.ValueKind != JsonValueKind.Array)
            throw new SpecificationParseException("Expected a JSON array at the root.");

        List<FileSpecification> list = new List<FileSpecification>();
        foreach (JsonElement element in root.RootElement.EnumerateArray())
        {
            cancellationToken.ThrowIfCancellationRequested();
            list.Add(ParseSingle(element, null, mapping));
        }

        return list;
    }

    private static FileSpecification ParseSingle(JsonElement element, string? fallbackLogicalName, SpecificationColumnMapping mapping)
    {
        if (element.ValueKind != JsonValueKind.Object)
            throw new SpecificationParseException("Each specification entry must be a JSON object.");

        string? logicalName = element.TryGetProperty(mapping.LogicalName, out JsonElement ln)
            ? ln.GetString()
            : fallbackLogicalName;

        if (string.IsNullOrWhiteSpace(logicalName))
            throw new SpecificationParseException("logicalName is required (either as a property or implied by the file name).");

        if (!element.TryGetProperty(mapping.Fields, out JsonElement fieldsElement) || fieldsElement.ValueKind != JsonValueKind.Array)
            throw new SpecificationParseException("fields array is required.");

        List<FieldSpecification> fields = new List<FieldSpecification>();
        foreach (JsonElement field in fieldsElement.EnumerateArray())
        {
            string? name = field.GetProperty(mapping.Name).GetString();
            string? type = field.GetProperty(mapping.Type).GetString();
            string? required = GetScalarAsText(field, mapping.Required);
            string? range = TryGetString(field, mapping.Range);
            string? def = TryGetString(field, mapping.DefaultValue);
            string? nullRaw = GetScalarAsText(field, mapping.NullValue);

            fields.Add(FieldSpecificationFactory.Create(
                name ?? string.Empty,
                required,
                type ?? string.Empty,
                range,
                def,
                nullRaw));
        }

        return new FileSpecification(logicalName, fields);
    }

    private static string? TryGetString(JsonElement element, string name) =>
        element.TryGetProperty(name, out JsonElement p) ? p.ToString() : null;

    private static string? GetScalarAsText(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out JsonElement p))
            return null;

        return p.ValueKind switch
        {
            JsonValueKind.String => p.GetString(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Number => p.GetRawText(),
            _ => p.ToString(),
        };
    }
}
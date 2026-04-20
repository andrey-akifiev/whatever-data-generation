using System.Globalization;

using CsvHelper;
using CsvHelper.Configuration;

using Whatever.TestData.Generator.Validation.Abstractions;
using Whatever.TestData.Generator.Validation.Specifications.Exceptions;

namespace Whatever.TestData.Generator.Validation.Specifications.Readers;

/// <summary>
/// Reads field specifications from a CSV file. One CSV file always maps to one logical dataset.
/// </summary>
public sealed class CsvSpecificationReader : ISpecificationReader
{
    private static readonly string[] Extensions = [".csv"];

    /// <inheritdoc />
    public IReadOnlyCollection<string> SupportedExtensions => Extensions;

    /// <inheritdoc />
    public Task<FileSpecification> ReadFileAsync(string filePath, SpecificationReaderOptions options, CancellationToken cancellationToken = default)
    {
        string logicalName = Path.GetFileNameWithoutExtension(filePath);
        SpecificationColumnMapping mapping = options.ColumnMapping;
        using StreamReader reader = new StreamReader(filePath);
        using CsvReader csv = new (reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            BadDataFound = null,
        });

        if (!csv.Read())
            throw new SpecificationParseException($"Specification CSV is empty: '{filePath}'.");

        csv.ReadHeader();
        string[] header = csv.HeaderRecord ?? Array.Empty<string>();
        Dictionary<string, int> map = SpecificationHeaderMapper.FromCsvHeader(header, mapping);

        List<FieldSpecification> fields = new List<FieldSpecification>();
        while (csv.Read())
        {
            cancellationToken.ThrowIfCancellationRequested();

            string? name = csv.GetField(map[mapping.Name])?.Trim();
            if (string.IsNullOrEmpty(name))
                continue;

            FieldSpecification field = FieldSpecificationFactory.Create(
                name,
                map.TryGetValue(mapping.Required, out int req) ? csv.GetField(req) : null,
                csv.GetField(map[mapping.Type]) ?? string.Empty,
                map.TryGetValue(mapping.Range, out int rIdx) ? csv.GetField(rIdx) : null,
                map.TryGetValue(mapping.DefaultValue, out int dIdx) ? csv.GetField(dIdx) : null,
                map.TryGetValue(mapping.NullValue, out int nIdx) ? csv.GetField(nIdx) : null);

            fields.Add(field);
        }

        if (fields.Count == 0)
            throw new SpecificationParseException($"No field rows found in '{filePath}'.");

        return Task.FromResult(new FileSpecification(logicalName, fields));
    }
}
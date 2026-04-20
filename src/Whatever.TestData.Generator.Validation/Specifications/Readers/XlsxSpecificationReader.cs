using ClosedXML.Excel;

using Whatever.TestData.Generator.Validation.Abstractions;
using Whatever.TestData.Generator.Validation.Specifications.Exceptions;

namespace Whatever.TestData.Generator.Validation.Specifications.Readers;
/// <summary>
/// Reads one or more logical specifications from an XLSX workbook (one worksheet per dataset).
/// </summary>
public sealed class XlsxSpecificationReader : ISpecificationReader
{
    private static readonly string[] Extensions = [".xlsx"];

    /// <inheritdoc />
    public IReadOnlyCollection<string> SupportedExtensions => Extensions;

    /// <inheritdoc />
    public Task<FileSpecification> ReadFileAsync(string filePath, SpecificationReaderOptions options, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<FileSpecification> all = ReadWorkbook(filePath, options);
        return Task.FromResult(all[0]);
    }

    /// <summary>
    /// Reads all requested worksheets as separate <see cref="FileSpecification"/> instances.
    /// </summary>
    public IReadOnlyList<FileSpecification> ReadWorkbook(string filePath, SpecificationReaderOptions options)
    {
        using XLWorkbook workbook = new XLWorkbook(filePath);
        IReadOnlyList<string>? filter = options.SheetFilter;
        SpecificationColumnMapping mapping = options.ColumnMapping;
        List<FileSpecification> result = new List<FileSpecification>();

        foreach (IXLWorksheet worksheet in workbook.Worksheets)
        {
            if (filter is { Count: > 0 } &&
                !filter.Any(n => n.Equals(worksheet.Name, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            result.Add(ReadSheet(worksheet, mapping));
        }

        if (result.Count == 0)
            throw new SpecificationParseException("No worksheets matched the filter.");

        return result;
    }

    private static FileSpecification ReadSheet(IXLWorksheet sheet, SpecificationColumnMapping mapping)
    {
        IXLRow? firstRow = sheet.FirstRowUsed();
        if (firstRow is null)
            throw new SpecificationParseException($"Worksheet '{sheet.Name}' is empty.");

        int headerRow = firstRow.RowNumber();
        Dictionary<string, int> map = SpecificationHeaderMapper.FromXlsxHeaderRow(sheet.Row(headerRow), mapping);

        List<FieldSpecification> fields = new List<FieldSpecification>();
        int lastRow = sheet.LastRowUsed()?.RowNumber() ?? headerRow;
        for (int r = headerRow + 1; r <= lastRow; r++)
        {
            string? Get(string column) =>
                map.TryGetValue(column, out int col) ? sheet.Cell(r, col).GetString() : null;

            string? name = Get(mapping.Name)?.Trim();
            if (string.IsNullOrEmpty(name))
                continue;

            FieldSpecification field = FieldSpecificationFactory.Create(
                name,
                Get(mapping.Required),
                Get(mapping.Type) ?? string.Empty,
                map.ContainsKey(mapping.Range) ? Get(mapping.Range) : null,
                map.ContainsKey(mapping.DefaultValue) ? Get(mapping.DefaultValue) : null,
                map.ContainsKey(mapping.NullValue) ? Get(mapping.NullValue) : null);

            fields.Add(field);
        }

        if (fields.Count == 0)
            throw new SpecificationParseException($"No field rows found in worksheet '{sheet.Name}'.");

        return new FileSpecification(sheet.Name, fields);
    }
}
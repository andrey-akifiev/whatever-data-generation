using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using Whatever.TestData.Generator.Validation.Abstractions;

namespace Whatever.TestData.Generator.Validation.Output;

/// <summary>
/// Writes a single-row XLSX workbook.
/// </summary>
public sealed class XlsxDatasetWriter(ILogger<XlsxDatasetWriter>? logger)
    : IFileDatasetWriter
{
    /// <inheritdoc />
    public GeneratedDataFormat Format => GeneratedDataFormat.Xlsx;

    /// <inheritdoc />
    public Task WriteAsync(
        string filePath,
        FileSpecification specification,
        IReadOnlyDictionary<string, string?> row,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);
        ArgumentNullException.ThrowIfNull(row);

        using XLWorkbook workbook = new XLWorkbook();
        IXLWorksheet? worksheet = workbook.AddWorksheet("data");

        List<string> columns = specification.Fields.Where(f => row.ContainsKey(f.Name)).Select(f => f.Name).ToList();
        for (int i = 0; i < columns.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            worksheet.Cell(1, i + 1).Value = columns[i];
            row.TryGetValue(columns[i], out string? value);
            worksheet.Cell(2, i + 1).Value = value ?? string.Empty;
        }

        workbook.SaveAs(filePath);
        logger?.LogDebug("Wrote XLSX dataset to {Path}", filePath);
        return Task.CompletedTask;
    }
}
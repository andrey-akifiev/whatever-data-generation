using ClosedXML.Excel;

using Whatever.TestData.Generator.Validation.Specifications.Exceptions;

namespace Whatever.TestData.Generator.Validation.Specifications.Readers;

internal static class SpecificationHeaderMapper
{
    public static Dictionary<string, int> FromCsvHeader(string[] header, SpecificationColumnMapping mapping)
    {
        Dictionary<string, int> map = new (StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < header.Length; i++)
        {
            string? key = header[i]?.Trim();
            if (string.IsNullOrEmpty(key))
                continue;

            map[key] = i;
        }

        Validate(map, mapping);
        return map;
    }

    public static Dictionary<string, int> FromXlsxHeaderRow(IXLRow headerRow, SpecificationColumnMapping mapping)
    {
        Dictionary<string, int> map = new (StringComparer.OrdinalIgnoreCase);
        foreach (IXLCell? cell in headerRow.Cells())
        {
            string text = cell.GetString().Trim();
            if (!string.IsNullOrEmpty(text))
            {
                map[text] = cell.Address.ColumnNumber;
            }
        }

        Validate(map, mapping);
        return map;
    }

    private static void Validate(Dictionary<string, int> map, SpecificationColumnMapping mapping)
    {
        foreach (string column in new[] { mapping.Name, mapping.Required, mapping.Type })
        {
            if (!map.ContainsKey(column))
            {
                throw new SpecificationParseException($"Missing required column '{column}'.");
            }
        }
    }
}
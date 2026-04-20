using ClosedXML.Excel;

using NUnit.Framework;

using Whatever.TestData.Generator.Validation.Abstractions;
using Whatever.TestData.Generator.Validation.Specifications.Readers;

namespace Whatever.TestData.Generator.Validation.Tests.Specifications.Readers;

public sealed class XlsxSpecificationReaderTests
{
    [Test]
    public async Task ReadFileAsync_reads_first_selected_sheet()
    {
        SpecificationColumnMapping defaultMapping = SpecificationColumnMapping.Default;
        
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.xlsx");
        try
        {
            using (XLWorkbook workbook = new ())
            {
                IXLWorksheet? sheet = workbook.AddWorksheet("accounts");
                sheet.Cell(1, 1).Value = defaultMapping.Name;
                sheet.Cell(1, 2).Value = defaultMapping.Required;
                sheet.Cell(1, 3).Value = defaultMapping.Type;
                sheet.Cell(1, 4).Value = defaultMapping.Range;
                sheet.Cell(2, 1).Value = "abc";
                sheet.Cell(2, 2).Value = "Y";
                sheet.Cell(2, 3).Value = "int";
                sheet.Cell(2, 4).Value = "[3,20]";
                workbook.SaveAs(path);
            }

            XlsxSpecificationReader reader = new ();
            FileSpecification spec = await reader.ReadFileAsync(path, new SpecificationReaderOptions());

            Assert.That(spec.LogicalName, Is.EqualTo("accounts"));
            Assert.That(spec.Fields.Count, Is.EqualTo(1));
            Assert.That(
                spec.Fields[0], 
                Is
                    .EqualTo(
                        new FieldSpecification(
                            "abc",
                            true,
                            new DataType(DataTypeKind.Integral),
                            "[3,20]",
                            null,
                            false,
                            new RangeConstraint(
                                false,
                                new NumericInterval(
                                    new NumericEndpoint(false, 3, true),
                                    new NumericEndpoint(false, 20, true)),
                                null,
                                null)))
                    .UsingPropertiesComparer());
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Test]
    public async Task ReadFileAsync_supports_custom_column_mapping()
    {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.xlsx");
        try
        {
            using (XLWorkbook workbook = new XLWorkbook())
            {
                IXLWorksheet sheet = workbook.AddWorksheet("accounts");
                sheet.Cell(1, 1).Value = "FieldName";
                sheet.Cell(1, 2).Value = "Mandatory";
                sheet.Cell(1, 3).Value = "DataType";
                sheet.Cell(1, 4).Value = "Allowed";
                sheet.Cell(2, 1).Value = "abc";
                sheet.Cell(2, 2).Value = "Y";
                sheet.Cell(2, 3).Value = "int";
                sheet.Cell(2, 4).Value = "[3,20]";
                workbook.SaveAs(path);
            }

            XlsxSpecificationReader reader = new XlsxSpecificationReader();
            SpecificationReaderOptions options = new SpecificationReaderOptions
            {
                ColumnMapping = new SpecificationColumnMapping
                {
                    Name = "FieldName",
                    Required = "Mandatory",
                    Type = "DataType",
                    Range = "Allowed",
                },
            };

            FileSpecification spec = await reader.ReadFileAsync(path, options);
            Assert.That(spec.Fields[0].Name, Is.EqualTo("abc"));
            Assert.That(spec.Fields[0].DataType.Kind, Is.EqualTo(DataTypeKind.Integral));
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
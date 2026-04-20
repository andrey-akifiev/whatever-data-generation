using NUnit.Framework;

using Whatever.TestData.Generator.Validation.Abstractions;
using Whatever.TestData.Generator.Validation.Specifications.Exceptions;
using Whatever.TestData.Generator.Validation.Specifications.Parsers;

namespace Whatever.TestData.Generator.Validation.Tests.Specifications.Parsers;

public sealed class DataTypeParserTests
{
    [TestCase("int")]
    [TestCase("INT")]
    [TestCase("integer")]
    [TestCase("INTEGER")]
    [TestCase("smallint")]
    [TestCase("SMALLINT")]
    [TestCase("tinyint")]
    [TestCase("TINYINT")]
    public void Parse_ShouldParseIntegerFromAllowedFormats(string specification)
    {
        DataType type = DataTypeParser.Parse(specification);
        Assert.That(type.Kind, Is.EqualTo(DataTypeKind.Integral));
        Assert.That(type.MaxLength, Is.Null);
        Assert.That(type.Precision, Is.Null);
        Assert.That(type.Scale, Is.Null);
    }
    
    [TestCase("nvarchar(10)", 10)]
    [TestCase("nvarchar(100)", 100)]
    [TestCase("NVARCHAR(100)", 100)]
    [TestCase("varchar(100)", 100)]
    [TestCase("VARCHAR(100)", 100)]
    [TestCase("string", int.MaxValue)]
    [TestCase("string(100)", 100)]
    [TestCase("STRING(100)", 100)]
    public void Parse_ShouldParseStringWithSizeFromAllowedFormats(string specification, int expectedLength)
    {
        DataType type = DataTypeParser.Parse(specification);
        Assert.That(type.Kind, Is.EqualTo(DataTypeKind.String));
        Assert.That(type.MaxLength, Is.EqualTo(expectedLength));
        Assert.That(type.Precision, Is.Null);
        Assert.That(type.Scale, Is.Null);
    }

    [TestCase("decimal", null, null)]
    [TestCase("decimal(12,4)", 12, 4)]
    [TestCase("float", null, null)]
    public void Parse_ShouldParseDecimalWithPrecisionAndScaleFromAllowedFormats(
        string specification,
        int? expectedPrecision,
        int? expectedScale)
    {
        DataType type = DataTypeParser.Parse(specification);
        Assert.That(type.Kind, Is.EqualTo(DataTypeKind.Floating));
        Assert.That(type.MaxLength, Is.Null);
        Assert.That(type.Precision, Is.EqualTo(expectedPrecision));
        Assert.That(type.Scale, Is.EqualTo(expectedScale));
    }

    [TestCase("guid")]
    [TestCase("uniqueidentifier")]
    [TestCase("uuid")]
    public void Parse_ShouldParseGuidFromAllowedFormats(string raw)
    {
        DataType type = DataTypeParser.Parse(raw);
        Assert.That(type.Kind, Is.EqualTo(DataTypeKind.Guid));
        Assert.That(type.MaxLength, Is.Null);
        Assert.That(type.Precision, Is.Null);
        Assert.That(type.Scale, Is.Null);
    }

    [Test]
    public void Parse_ShouldThrowWhenUnsupportedDataType()
    {
        Assert.Throws<SpecificationParseException>(() => DataTypeParser.Parse("money"));
    }
}
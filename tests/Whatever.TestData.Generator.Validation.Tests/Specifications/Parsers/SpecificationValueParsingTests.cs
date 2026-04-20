using NUnit.Framework;

using Whatever.TestData.Generator.Validation.Specifications.Exceptions;
using Whatever.TestData.Generator.Validation.Specifications.Parsers;

namespace Whatever.TestData.Generator.Validation.Tests.Specifications.Parsers;

public sealed class SpecificationValueParsingTests
{
    [TestCase("Y", true)]
    [TestCase("REQUIRED", true)]
    [TestCase("1", true)]
    [TestCase("true", true)]
    [TestCase("N", false)]
    [TestCase("OPTIONAL", false)]
    [TestCase("0", false)]
    [TestCase("false", false)]
    [TestCase(null, false)]
    public void ParseRequired_ShouldSupportAllowedValues(string? raw, bool expected)
    {
        bool actual = SpecificationValueParsing.ParseRequired(raw);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [TestCase("Y", true)]
    [TestCase("YES", true)]
    [TestCase("1", true)]
    [TestCase("true", true)]
    [TestCase("N", false)]
    [TestCase("NO", false)]
    [TestCase("0", false)]
    [TestCase("false", false)]
    [TestCase(null, false)]
    public void ParseAllowsNull_ShouldSupportAllowedValues(string? raw, bool expected)
    {
        bool actual = SpecificationValueParsing.ParseAllowsNull(raw);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void ParseRequired_ShouldThrowWhenUnknownToken()
    {
        Assert.Throws<SpecificationParseException>(() => SpecificationValueParsing.ParseRequired("MAYBE"));
    }
}
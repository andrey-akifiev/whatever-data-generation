using NUnit.Framework;

using Whatever.TestData.Generator.Validation.Abstractions;
using Whatever.TestData.Generator.Validation.Specifications.Parsers;

namespace Whatever.TestData.Generator.Validation.Tests.Specifications.Parsers;

public sealed class RangeConstraintParserTests
{
    [TestCase("[3,20]", 3, true, 20, true)]
    [TestCase("[3,20)", 3, true, 20, false)]
    [TestCase("(3,20]", 3, false, 20, true)]
    [TestCase("(3,20)", 3, false, 20, false)]
    public void Parse_ShouldSupportBoundedIntegerIntervals(
        string specification,
        int? expectedMinValue,
        bool? expectedMinInclusive,
        int? expectedMaxValue,
        bool? expectedMaxInclusive)
    {
        DataType type = DataTypeParser.Parse("int");
        RangeConstraint range = RangeConstraintParser.Parse(specification, type);

        Assert.That(range.DiscreteValues, Is.Null);
        Assert.That(range.NullListedInRange, Is.False);
        Assert.That(range.NumericInterval, Is.Not.Null);
        Assert.That(range.StringLengthInterval, Is.Null);
        
        NumericInterval interval = range.NumericInterval!.Value;
        Assert.That(interval.Min.IsUnbounded, Is.False);
        Assert.That(interval.Min.Value, Is.EqualTo(expectedMinValue));
        Assert.That(interval.Min.Inclusive, Is.EqualTo(expectedMinInclusive));
        Assert.That(interval.Max.IsUnbounded, Is.False);
        Assert.That(interval.Max.Value, Is.EqualTo(expectedMaxValue));
        Assert.That(interval.Max.Inclusive, Is.EqualTo(expectedMaxInclusive));
    }

    [TestCase("(,20)", true, 0, false, false, 20, false)]
    [TestCase("[,20]", true, 0, true, false, 20, true)]
    [TestCase("(3,)", false, 3, false, true, 0, false)]
    [TestCase("[3,)", false, 3, true, true, 0, false)]
    [TestCase("(,)", true, 0, false, true, 0, false)]
    [TestCase("[,]", true, 0, true, true, 0, true)]
    public void Parse_ShouldSupportUnboundedIntegerIntervals(
        string specification,
        bool isMinUnbounded,
        int expectedMinValue,
        bool expectedMinInclusive,
        bool isMaxUnbounded,
        int expectedMaxValue,
        bool expectedMaxInclusive)
    {
        DataType type = DataTypeParser.Parse("int");
        RangeConstraint range = RangeConstraintParser.Parse(specification, type);

        Assert.That(range.NullListedInRange, Is.False);
        Assert.That(range.NumericInterval, Is.Not.Null);
        Assert.That(range.StringLengthInterval, Is.Null);
        
        NumericInterval interval = range.NumericInterval!.Value;
        Assert.That(interval.Min.IsUnbounded, Is.EqualTo(isMinUnbounded));
        Assert.That(interval.Min.Value, Is.EqualTo(expectedMinValue));
        Assert.That(interval.Min.Inclusive, Is.EqualTo(expectedMinInclusive));
        Assert.That(interval.Max.IsUnbounded, Is.EqualTo(isMaxUnbounded));
        Assert.That(interval.Max.Value, Is.EqualTo(expectedMaxValue));
        Assert.That(interval.Max.Inclusive, Is.EqualTo(expectedMaxInclusive));
    }

    [TestCase("[3.1,20.1]", 3.1, true, 20.1, true)]
    [TestCase("[3.2,20.2)", 3.2, true, 20.2, false)]
    [TestCase("(3.3,20.3]", 3.3, false, 20.3, true)]
    [TestCase("(3.4,20.4)", 3.4, false, 20.4, false)]
    public void Parse_ShouldSupportBoundedFloatingPointIntervals(
        string specification,
        decimal? expectedMinValue,
        bool? expectedMinInclusive,
        decimal? expectedMaxValue,
        bool? expectedMaxInclusive)
    {
        DataType type = DataTypeParser.Parse("float");
        RangeConstraint range = RangeConstraintParser.Parse(specification, type);

        Assert.That(range.NullListedInRange, Is.False);
        Assert.That(range.NumericInterval, Is.Not.Null);
        Assert.That(range.StringLengthInterval, Is.Null);
        
        NumericInterval interval = range.NumericInterval!.Value;
        Assert.That(interval.Min.IsUnbounded, Is.False);
        Assert.That(interval.Min.Value, Is.EqualTo(expectedMinValue));
        Assert.That(interval.Min.Inclusive, Is.EqualTo(expectedMinInclusive));
        Assert.That(interval.Max.IsUnbounded, Is.False);
        Assert.That(interval.Max.Value, Is.EqualTo(expectedMaxValue));
        Assert.That(interval.Max.Inclusive, Is.EqualTo(expectedMaxInclusive));
    }

    [TestCase("[3,20]", 3, true, 20, true)]
    public void Parse_ShouldSupportBoundedStringIntervals(
        string specification,
        int? expectedMinValue,
        bool? expectedMinInclusive,
        int? expectedMaxValue,
        bool? expectedMaxInclusive
    )
    {
        DataType type = DataTypeParser.Parse("string");
        
        RangeConstraint range = RangeConstraintParser.Parse(specification, type);
        
        Assert.That(range.NullListedInRange, Is.False);
        Assert.That(range.NumericInterval, Is.Null);
        Assert.That(range.StringLengthInterval, Is.Not.Null);
        
        NumericInterval interval = range.StringLengthInterval!.Value;
        Assert.That(interval.Min.IsUnbounded, Is.False);
        Assert.That(interval.Min.Value, Is.EqualTo(expectedMinValue));
        Assert.That(interval.Min.Inclusive, Is.EqualTo(expectedMinInclusive));
        Assert.That(interval.Max.IsUnbounded, Is.False);
        Assert.That(interval.Max.Value, Is.EqualTo(expectedMaxValue));
        Assert.That(interval.Max.Inclusive, Is.EqualTo(expectedMaxInclusive));
    }

    [TestCase("{1}", "1")]
    [TestCase("{1,2}", "1", "2")]
    [TestCase("{1,2,3}", "1", "2", "3")]
    public void Parse_ShouldSupportDiscreteIntegerValues(string specification, params string[] values)
    {
        DataType type = DataTypeParser.Parse("int");
        RangeConstraint range = RangeConstraintParser.Parse(specification, type);

        Assert.That(range.NullListedInRange, Is.False);
        Assert.That(range.NumericInterval, Is.Null);
        Assert.That(range.StringLengthInterval, Is.Null);
        Assert.That(range.DiscreteValues, Is.EquivalentTo(values));
    }
    
    [TestCase("{NULL}")]
    public void Parse_ShouldSupportDiscreteNullValue(string specification)
    {
        DataType type = DataTypeParser.Parse("int");
        RangeConstraint range = RangeConstraintParser.Parse(specification, type);

        Assert.That(range.NullListedInRange, Is.True);
        Assert.That(range.NumericInterval, Is.Null);
        Assert.That(range.StringLengthInterval, Is.Null);
        Assert.That(range.DiscreteValues, Is.Null);
    }
    
    [TestCase("{NULL,1}", "1")]
    [TestCase("{NULL,1,2}", "1", "2")]
    [TestCase("{NULL},{1}", "1")]
    [TestCase("{NULL},{1,2}", "1", "2")]
    public void Parse_ShouldSupportDiscreteNullAndIntegerValues(string specification, params string[] values)
    {
        DataType type = DataTypeParser.Parse("int");
        RangeConstraint range = RangeConstraintParser.Parse(specification, type);

        Assert.That(range.NullListedInRange, Is.True);
        Assert.That(range.NumericInterval, Is.Null);
        Assert.That(range.StringLengthInterval, Is.Null);
        Assert.That(range.DiscreteValues, Is.EquivalentTo(values));
    }

    [TestCase("{NULL},[3,20]", 3, true, 20, true)]
    [TestCase("{NULL},(3,20)", 3, false, 20, false)]
    public void Parse_ShouldSupportDiscreteNullAndIntegerIntervalValues(
        string specification,
        int? expectedMinValue,
        bool? expectedMinInclusive,
        int? expectedMaxValue,
        bool? expectedMaxInclusive)
    {
        DataType type = DataTypeParser.Parse("int");
        RangeConstraint range = RangeConstraintParser.Parse(specification, type);

        Assert.That(range.DiscreteValues, Is.Null);
        Assert.That(range.NullListedInRange, Is.True);
        Assert.That(range.NumericInterval, Is.Not.Null);
        Assert.That(range.StringLengthInterval, Is.Null);
        
        NumericInterval interval = range.NumericInterval!.Value;
        Assert.That(interval.Min.IsUnbounded, Is.False);
        Assert.That(interval.Min.Value, Is.EqualTo(expectedMinValue));
        Assert.That(interval.Min.Inclusive, Is.EqualTo(expectedMinInclusive));
        Assert.That(interval.Max.IsUnbounded, Is.False);
        Assert.That(interval.Max.Value, Is.EqualTo(expectedMaxValue));
        Assert.That(interval.Max.Inclusive, Is.EqualTo(expectedMaxInclusive));
    }
}
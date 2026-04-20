namespace Whatever.TestData.Generator.Validation.Abstractions;

/// <summary>
/// Parsed value of the <c>Range</c> column, including optional NULL token and discrete sets.
/// </summary>
public sealed class RangeConstraint(
    bool nullListedInRange,
    NumericInterval? numericInterval,
    NumericInterval? stringLengthInterval,
    IReadOnlyList<string>? discreteValues)
{
    /// <summary>
    /// True when the range text explicitly contains a <c>{NULL}</c> segment.<br/>
    /// If so, <c>NULL</c> value will be excluded from <see cref="DiscreteValues"/> collection.
    /// </summary>
    public bool NullListedInRange { get; } = nullListedInRange;

    /// <summary>
    /// Parsed numeric interval when the range text contains a valid interval segment.<br/>
    /// If the range text does not contain a valid interval segment, this property is null.
    /// </summary>
    public NumericInterval? NumericInterval { get; } = numericInterval;

    /// <summary>
    /// Parsed string length interval when the range text contains a valid string length segment (e.g. <c>[3,20]</c>).<br/>
    /// If the range text does not contain a valid string length segment, this property is null.
    /// </summary>
    public NumericInterval? StringLengthInterval { get; } = stringLengthInterval;

    /// <summary>
    /// Parsed discrete values when the range text contains valid discrete value segments (e.g. <c>{A,B,C}</c>).<br/>
    /// If the range text does not contain valid discrete value segments, this property is null.<br/>
    /// This property is also null, when original range text contains only a <c>{NULL}</c> segment,<br/>
    /// since in that case <see cref="NullListedInRange"/> is true and there are no other discrete values.
    /// </summary>
    public IReadOnlyList<string>? DiscreteValues { get; } = discreteValues;
}
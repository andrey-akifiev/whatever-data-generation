namespace Whatever.TestData.Generator.Validation.Abstractions;

/// <summary>
/// One bound of an interval, possibly unbounded.
/// </summary>
public readonly struct NumericEndpoint(bool isUnbounded, decimal value, bool inclusive)
{
    /// <summary>
    /// True when this endpoint represents an unbounded limit (e.g. potential positive or negative infinity).<br/>
    /// Usually, unbounded range is treated as bounded to extremums of particular data type,<br/>
    /// such as <c>int.MaxValue</c> and <c>int.MinValue</c> for integer type.
    /// <example>
    /// (,) - exclusive unbounded range<br/>
    /// [,] - inclusive unbounded range
    /// </example>
    /// </summary>
    public bool IsUnbounded { get; } = isUnbounded;

    /// <summary>
    /// The numeric value of this endpoint when it is bounded.<br/>
    /// The value is ignored when <see cref="IsUnbounded"/> is true.
    /// </summary>
    public decimal Value { get; } = value;

    /// <summary>
    /// True when the endpoint is inclusive (i.e. a closed interval),<br/>
    /// false when exclusive (i.e. an open interval).<br/>
    /// This property is ignored when <see cref="IsUnbounded"/> is true.
    /// <example>
    /// [3,20] - inclusive<br/>
    /// (3,20) - exclusive
    /// </example>
    /// </summary>
    public bool Inclusive { get; } = inclusive;
}
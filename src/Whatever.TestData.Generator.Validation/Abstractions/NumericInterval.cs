namespace Whatever.TestData.Generator.Validation.Abstractions;

/// <summary>
/// Numeric or string-length interval using mathematical bracket (interval) notation.
/// </summary>
public readonly struct NumericInterval(NumericEndpoint min, NumericEndpoint max)
{
    /// <summary>
    /// The minimum endpoint of the interval.<br/>
    /// If <see cref="NumericEndpoint.IsUnbounded"/> is true, the value is ignored<br/>
    /// and the interval extends to minimal possible value for particular data type.
    /// </summary>
    public NumericEndpoint Min { get; } = min;

    /// <summary>
    /// The maximum endpoint of the interval.<br/>
    /// If <see cref="NumericEndpoint.IsUnbounded"/> is true, the value is ignored<br/>
    /// and the interval extends to maximal possible value for particular data type.
    /// </summary>
    public NumericEndpoint Max { get; } = max;
}
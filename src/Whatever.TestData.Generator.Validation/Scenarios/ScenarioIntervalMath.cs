using Whatever.TestData.Generator.Validation.Abstractions;

namespace Whatever.TestData.Generator.Validation.Scenarios;

internal static class ScenarioIntervalMath
{
    public static (long Min, long Max) ToInclusiveLongBounds(NumericInterval interval)
    {
        long min = ToBound(interval.Min, isLower: true);
        long max = ToBound(interval.Max, isLower: false);
        if (min > max)
        {
            (min, max) = (max, min);
        }

        return (min, max);
    }

    private static long ToBound(NumericEndpoint endpoint, bool isLower)
    {
        if (endpoint.IsUnbounded)
        {
            return isLower ? long.MinValue : long.MaxValue;
        }

        decimal raw = endpoint.Value;
        if (endpoint.Inclusive)
        {
            return isLower ? CeilingToLong(raw) : FloorToLong(raw);
        }

        return isLower ? FloorToLong(raw) + 1 : CeilingToLong(raw) - 1;
    }

    private static long FloorToLong(decimal value)
        => (long)Math.Floor((double)value);

    private static long CeilingToLong(decimal value)
        => (long)Math.Ceiling((double)value);
}
using Whatever.TestData.Generator.Validation.Abstractions;

namespace Whatever.TestData.Generator.Validation.Scenarios;

internal static class ValidValueSampler
{
    public static string SampleValidString(FieldSpecification field)
    {
        int maxLenFromType = field.DataType.MaxLength ?? int.MaxValue;
        int minLen;
        int maxLen;

        if (field.Range.StringLengthInterval is { } len)
        {
            (long a, long b) = ScenarioIntervalMath.ToInclusiveLongBounds(len);
            minLen = (int)Math.Clamp(a, 0, int.MaxValue);
            maxLen = (int)Math.Clamp(b, minLen, int.MaxValue);
        }
        else
        {
            minLen = 0;
            maxLen = Math.Min(maxLenFromType, 32);
        }

        if (field.Range.DiscreteValues is { Count: > 0 } discrete)
            return discrete[0];

        int target = Math.Min(maxLen, Math.Max(minLen, minLen + (maxLen - minLen) / 2));
        target = Math.Clamp(target, minLen, Math.Min(maxLen, maxLenFromType));
        return new string('a', target);
    }

    public static string SampleValidIntegral(FieldSpecification field)
    {
        if (field.Range.DiscreteValues is { Count: > 0 } discrete)
        {
            foreach (string token in discrete)
            {
                if (long.TryParse(token, out long v))
                    return v.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        if (field.Range.NumericInterval is { } interval)
        {
            (long min, long max) = ScenarioIntervalMath.ToInclusiveLongBounds(interval);
            if (min > max)
                (min, max) = (max, min);

            long mid = min + (max - min) / 2;
            mid = Math.Clamp(mid, min, max);
            return mid.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        return "0";
    }

    public static string SampleValidFloating(FieldSpecification field)
    {
        if (field.Range.NumericInterval is { } interval)
        {
            (long min, long max) = ScenarioIntervalMath.ToInclusiveLongBounds(interval);
            double mid = min + (max - min) / 2.0;
            return mid.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        return "0.0";
    }

    public static string SampleValidBoolean(FieldSpecification field)
    {
        _ = field;
        return "false";
    }

    public static string SampleValidDateTime(FieldSpecification field)
    {
        _ = field;
        return "2026-04-18T10:01:30Z";
    }

    public static string SampleValidGuid(FieldSpecification field)
    {
        _ = field;
        return Guid.NewGuid().ToString("D");
    }
}
using System.Globalization;

using Whatever.TestData.Generator.Validation.Abstractions;

namespace Whatever.TestData.Generator.Validation.Scenarios;

/// <inheritdoc />
public sealed class ScenarioPlanner : IScenarioPlanner
{
    private const string WrongDateLiteral = "2026-04-18T10:01:30Z";

    /// <inheritdoc />
    public IReadOnlyList<TestScenarioDescriptor> Plan(FileSpecification file, FieldSpecification field, ScenarioSurfaceKind surface)
    {
        List<TestScenarioDescriptor> scenarios = new List<TestScenarioDescriptor>();

        void Add(string slug, bool isPositive, ScenarioBodyKind body, string? literal = null)
        {
            string folder = $"{file.LogicalName}_{field.Name}_{slug}";
            scenarios.Add(new TestScenarioDescriptor(folder, isPositive, file.LogicalName, field.Name, body, literal));
        }

        bool supportsNullToken = surface is ScenarioSurfaceKind.Structured or ScenarioSurfaceKind.ColumnarNullable;

        if (supportsNullToken)
        {
            Add("null", field.AllowsNull, ScenarioBodyKind.NullLiteral);
        }

        Add("empty", false, ScenarioBodyKind.EmptyString);
        Add("space", false, ScenarioBodyKind.SingleSpace);
        Add("tab", false, ScenarioBodyKind.Tab);
        Add("newline", false, ScenarioBodyKind.Newline);

        if (field.IsRequired)
            Add("missed", false, ScenarioBodyKind.MissedField);

        switch (field.DataType.Kind)
        {
            case DataTypeKind.String:
                AddStringIntervalScenarios(file, field, Add);
                break;
            case DataTypeKind.Integral:
                AddIntegralScenarios(file, field, Add);
                break;
            case DataTypeKind.Floating:
                AddFloatingScenarios(file, field, Add);
                break;
            case DataTypeKind.Boolean:
                Add("true", false, ScenarioBodyKind.Literal, "true");
                Add("false", true, ScenarioBodyKind.Literal, "false");
                break;
            case DataTypeKind.DateTime:
                Add("valid", true, ScenarioBodyKind.Literal, WrongDateLiteral);
                Add("number", false, ScenarioBodyKind.Literal, "123");
                break;
            case DataTypeKind.Guid:
                Add("bad", false, ScenarioBodyKind.Literal, "not-a-guid");
                Add("good", true, ScenarioBodyKind.Literal, Guid.NewGuid().ToString("D"));
                break;
        }

        return scenarios;
    }

    private static void AddStringIntervalScenarios(
        FileSpecification file,
        FieldSpecification field,
        Action<string, bool, ScenarioBodyKind, string?> add)
    {
        if (field.Range.DiscreteValues is { Count: > 0 } discrete)
        {
            foreach (string value in discrete)
                add(SlugifyToken(value), true, ScenarioBodyKind.Literal, value);

            add("other", false, ScenarioBodyKind.Literal, "___");
            return;
        }

        if (field.Range.StringLengthInterval is null)
        {
            add("sample", true, ScenarioBodyKind.Literal, ValidValueSampler.SampleValidString(field));
            add("digits", false, ScenarioBodyKind.Literal, "123");
            add("bool", false, ScenarioBodyKind.Literal, "true");
            return;
        }

        (long min, long max) = ScenarioIntervalMath.ToInclusiveLongBounds(field.Range.StringLengthInterval.Value);
        if (min > max)
            (min, max) = (max, min);

        if (min > 0)
            add((min - 1).ToString(CultureInfo.InvariantCulture), false, ScenarioBodyKind.Literal, new string('x', (int)(min - 1)));

        add(min.ToString(CultureInfo.InvariantCulture), true, ScenarioBodyKind.Literal, new string('x', (int)min));
        add(max.ToString(CultureInfo.InvariantCulture), true, ScenarioBodyKind.Literal, new string('y', (int)max));

        if (max < int.MaxValue - 1)
            add((max + 1).ToString(CultureInfo.InvariantCulture), false, ScenarioBodyKind.Literal, new string('z', (int)(max + 1)));

        add("digits", false, ScenarioBodyKind.Literal, new string('1', (int)Math.Clamp(min, 1, 3)));
        add("bool", false, ScenarioBodyKind.Literal, "true");
    }

    private static void AddIntegralScenarios(
        FileSpecification file,
        FieldSpecification field,
        Action<string, bool, ScenarioBodyKind, string?> add)
    {
        _ = file;

        if (field.Range.DiscreteValues is { Count: > 0 } discrete)
        {
            List<long> numbers = discrete
                .Select(v => long.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out long n) ? (long?)n : null)
                .Where(n => n.HasValue)
                .Select(n => n!.Value)
                .OrderBy(n => n)
                .ToList();

            if (numbers.Count == 0)
            {
                add("bad", false, ScenarioBodyKind.Literal, "x");
                return;
            }

            foreach (long n in numbers)
                add(n.ToString(CultureInfo.InvariantCulture), true, ScenarioBodyKind.Literal, n.ToString(CultureInfo.InvariantCulture));

            add((numbers[0] - 1).ToString(CultureInfo.InvariantCulture), false, ScenarioBodyKind.Literal, (numbers[0] - 1).ToString(CultureInfo.InvariantCulture));
            add((numbers[^1] + 1).ToString(CultureInfo.InvariantCulture), false, ScenarioBodyKind.Literal, (numbers[^1] + 1).ToString(CultureInfo.InvariantCulture));
            add("string", false, ScenarioBodyKind.Literal, "fgh");
            add("bool", false, ScenarioBodyKind.Literal, "true");
            add("date", false, ScenarioBodyKind.Literal, WrongDateLiteral);
            return;
        }

        if (field.Range.NumericInterval is null)
        {
            add("sample", true, ScenarioBodyKind.Literal, ValidValueSampler.SampleValidIntegral(field));
            add("string", false, ScenarioBodyKind.Literal, "abc");
            return;
        }

        (long min, long max) = ScenarioIntervalMath.ToInclusiveLongBounds(field.Range.NumericInterval.Value);
        if (min > max)
            (min, max) = (max, min);

        if (min > long.MinValue)
            add((min - 1).ToString(CultureInfo.InvariantCulture), false, ScenarioBodyKind.Literal, (min - 1).ToString(CultureInfo.InvariantCulture));

        add(min.ToString(CultureInfo.InvariantCulture), true, ScenarioBodyKind.Literal, min.ToString(CultureInfo.InvariantCulture));
        add(max.ToString(CultureInfo.InvariantCulture), true, ScenarioBodyKind.Literal, max.ToString(CultureInfo.InvariantCulture));

        if (max < long.MaxValue)
            add((max + 1).ToString(CultureInfo.InvariantCulture), false, ScenarioBodyKind.Literal, (max + 1).ToString(CultureInfo.InvariantCulture));

        add("string", false, ScenarioBodyKind.Literal, "fgh");
        add("bool", false, ScenarioBodyKind.Literal, "true");
        add("date", false, ScenarioBodyKind.Literal, WrongDateLiteral);
    }

    private static void AddFloatingScenarios(
        FileSpecification file,
        FieldSpecification field,
        Action<string, bool, ScenarioBodyKind, string?> add)
    {
        _ = file;
        add("sample", true, ScenarioBodyKind.Literal, ValidValueSampler.SampleValidFloating(field));
        add("string", false, ScenarioBodyKind.Literal, "abc");
        add("bool", false, ScenarioBodyKind.Literal, "true");
    }

    private static string SlugifyToken(string token)
    {
        string trimmed = token.Trim();
        foreach (char invalid in Path.GetInvalidFileNameChars())
            trimmed = trimmed.Replace(invalid, '_');

        return string.IsNullOrWhiteSpace(trimmed) ? "value" : trimmed;
    }
}
using Whatever.TestData.Generator.Validation.Abstractions;
using Whatever.TestData.Generator.Validation.Specifications.Readers;

namespace Whatever.TestData.Generator.Validation.Specifications;

/// <summary>
/// Fluent builder for constructing <see cref="FileSpecification"/> instances in code.
/// </summary>
public sealed class SpecificationBuilder
{
    private string _logicalName = "dataset";
    private readonly List<FieldSpecification> _fields = new();

    /// <summary>
    /// Sets the logical file name used for generated outputs.
    /// </summary>
    public SpecificationBuilder ForFile(string logicalName)
    {
        _logicalName = logicalName;
        return this;
    }

    /// <summary>
    /// Adds a field using raw textual columns identical to CSV rows.
    /// </summary>
    public SpecificationBuilder AddField(
        string name,
        string? required,
        string type,
        string? range = null,
        string? defaultValue = null,
        string? nullValue = null)
    {
        _fields.Add(FieldSpecificationFactory.Create(name, required, type, range, defaultValue, nullValue));
        return this;
    }

    /// <summary>
    /// Builds the immutable specification instance.
    /// </summary>
    public FileSpecification Build()
    {
        if (_fields.Count == 0)
        {
            throw new InvalidOperationException("At least one field must be added.");
        }

        return new FileSpecification(_logicalName, _fields);
    }
}
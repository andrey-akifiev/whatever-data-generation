namespace Whatever.TestData.Generator.Validation.Abstractions;

/// <summary>
/// Describes a single column or JSON field in a generated dataset.
/// </summary>
public sealed class FieldSpecification(
    string name,
    bool isRequired,
    DataType dataType,
    string? rawRange,
    string? defaultValue,
    bool allowsNullFromColumn,
    RangeConstraint range)
{
    /// <summary>
    /// Physical column/property name.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// In datasets there are two different group of columns/properties.
    /// <list type="bullet">
    ///     <item>
    ///         Mandatory (required) - should always be present in generated datasets except for the dedicated missed-field scenario.
    ///         Otherwise, such dataset will be invalid and should be rejected by validation logic.
    ///     </item>
    ///     <item>
    ///         Optional (not required) - may be omitted from generated datasets in various scenarios.
    ///     </item>
    ///     </list>
    /// </summary>
    public bool IsRequired { get; } = isRequired;

    /// <summary>
    /// Data type of the field, which may be used for validation and value generation.<br/>
    /// It is derived from the raw type column in the specification and may contain additional information<br/>
    /// like maximum length for string types or precision/scale for decimals.
    /// </summary>
    public DataType DataType { get; } = dataType;

    /// <summary>
    /// Original raw range expression from the specification, which may be used for error messages or debugging purposes.
    /// <example>
    /// <c>"[3,20]"</c><br/>
    /// <c>"(3,20)"</c>
    /// </example>
    /// </summary>
    public string? RawRange { get; } = rawRange;

    /// <summary>
    /// Default value for the field when it is not provided in the dataset, which may be used for validation and value generation.<br/>
    /// It is derived from the default value column in the specification and may be null if not specified.
    /// </summary>
    public string? DefaultValue { get; } = defaultValue;

    /// <summary>
    /// Whether NULL is allowed according to the NULL column when present.
    /// </summary>
    public bool AllowsNullFromColumn { get; } = allowsNullFromColumn;

    /// <summary>
    /// Parsed range constraint for the field, which may be used for validation and value generation.
    /// </summary>
    public RangeConstraint Range { get; } = range;

    /// <summary>
    /// Effective NULL permission: column flag or explicit NULL token inside the range expression.
    /// </summary>
    public bool AllowsNull => AllowsNullFromColumn || Range.NullListedInRange;
}
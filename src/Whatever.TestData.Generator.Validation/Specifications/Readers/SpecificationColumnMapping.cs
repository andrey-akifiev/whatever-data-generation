namespace Whatever.TestData.Generator.Validation.Specifications.Readers;

/// <summary>
/// Describes physical names for specification columns/properties.
/// </summary>
public sealed class SpecificationColumnMapping
{
    /// <summary>
    /// Default mapping with commonly used column names, which can be used when specification files follow the standard naming convention.
    /// </summary>
    public static SpecificationColumnMapping Default { get; } = new();

    /// <summary>
    /// Physical column name for the field name column in the specification,<br/>
    /// which is used to read the field name for each field specification.
    /// </summary>
    public string Name { get; init; } = "name";

    /// <summary>
    /// Physical column name for the required/optional column in the specification,<br/>
    /// which is used to read whether the field is required or optional for each field specification.
    /// </summary>
    public string Required { get; init; } = "required";

    /// <summary>
    /// Physical column name for the data type column in the specification,<br/>
    /// which is used to read the data type for each field specification.
    /// </summary>
    public string Type { get; init; } = "type";

    /// <summary>
    /// Physical column name for the range column in the specification,<br/>
    /// which is used to read the range constraint for each field specification when applicable.<br/>
    /// </summary>
    public string Range { get; init; } = "range";

    /// <summary>
    /// Physical column name for the default value column in the specification,<br/>
    /// which is used to read the default value for each field specification when applicable.<br/>
    /// </summary>
    public string DefaultValue { get; init; } = "defaultValue";

    /// <summary>
    /// Physical column name for the NULL value column in the specification,<br/>
    /// which is used to read whether NULL values are allowed for each field specification when applicable.
    /// </summary>
    public string NullValue { get; init; } = "nullValue";

    /// <summary>
    /// Physical column name for the logical file name column in the specification,<br/>
    /// which is used to read the logical file name for the dataset specification when applicable.<br/>
    /// Applicable only for specifications set with JSON files.
    /// </summary>
    public string LogicalName { get; init; } = "logicalName";

    /// <summary>
    /// Physical column name for the fields collection column in the specification,<br/>
    /// which is used to read the collection of field specifications for the dataset specification when applicable.<br/>
    /// Applicable only for specifications set with JSON files.
    /// </summary>
    public string Fields { get; init; } = "fields";
}
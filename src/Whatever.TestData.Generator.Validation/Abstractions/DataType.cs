namespace Whatever.TestData.Generator.Validation.Abstractions;

/// <summary>
/// Represents a SQL-like column type from a specification row.
/// </summary>
public sealed class DataType(DataTypeKind kind, int? maxLength = null, int? precision = null, int? scale = null)
{
    /// <summary>
    /// Logical family of the type.
    /// </summary>
    public DataTypeKind Kind { get; } = kind;

    /// <summary>
    /// Optional maximum length for string types (for example NVARCHAR(100)).
    /// </summary>
    public int? MaxLength { get; } = maxLength;

    /// <summary>
    /// Optional DECIMAL/NUMERIC precision.
    /// </summary>
    public int? Precision { get; } = precision;

    /// <summary>
    /// Optional DECIMAL/NUMERIC scale.
    /// </summary>
    public int? Scale { get; } = scale;
}
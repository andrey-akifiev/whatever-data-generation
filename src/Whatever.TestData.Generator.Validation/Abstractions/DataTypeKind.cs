#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Whatever.TestData.Generator.Validation.Abstractions;

/// <summary>
/// High-level classification for SQL-like data types used in specifications.
/// </summary>
public enum DataTypeKind
{
    String,
    Integral,
    Floating,
    Boolean,
    DateTime,
    Guid,
}
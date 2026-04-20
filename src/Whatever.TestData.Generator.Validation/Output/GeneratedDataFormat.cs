#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Whatever.TestData.Generator.Validation.Output;

/// <summary>
/// Supported physical formats for generated datasets.
/// </summary>
public enum GeneratedDataFormat
{
    Csv,
    Xlsx,
    Json,
    Yaml,
    Parquet,
}
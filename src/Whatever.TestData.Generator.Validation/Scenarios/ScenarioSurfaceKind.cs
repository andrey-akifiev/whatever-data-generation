namespace Whatever.TestData.Generator.Validation.Scenarios;

/// <summary>
/// Describes how the consumer format represents values, which affects whether explicit NULL scenarios are generated.
/// </summary>
public enum ScenarioSurfaceKind
{
    /// <summary>
    /// CSV or XLSX style cells where explicit JSON null is not used.
    /// </summary>
    Tabular,

    /// <summary>
    /// JSON or YAML where null tokens are meaningful.
    /// </summary>
    Structured,

    /// <summary>
    /// Parquet columns may store nulls; treated like structured surfaces.
    /// </summary>
    ColumnarNullable,
}
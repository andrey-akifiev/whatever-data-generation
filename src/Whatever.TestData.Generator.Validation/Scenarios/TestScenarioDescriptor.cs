namespace Whatever.TestData.Generator.Validation.Scenarios;

/// <summary>
/// Describes a single generated scenario folder and how the target field should be mutated.
/// </summary>
public sealed class TestScenarioDescriptor(
    string folderName,
    bool isPositive,
    string targetLogicalFile,
    string targetField,
    ScenarioBodyKind bodyKind,
    string? literal = null)
{
    /// <summary>
    /// Directory name placed under the output root.
    /// </summary>
    public string FolderName { get; } = folderName;

    /// <summary>
    /// Indicates whether the scenario is expected to pass validation (true) or fail validation (false)<br/>
    /// based on the mutation applied to the target field.
    /// </summary>
    public bool IsPositive { get; } = isPositive;

    /// <summary>
    /// Logical name of the file containing the target field, which is mutated in this scenario.<br/>
    /// It corresponds to the logical name from the specification and is used to identify the file<br/>
    /// within the generated dataset where the scenario mutation should be applied.
    /// </summary>
    public string TargetLogicalFile { get; } = targetLogicalFile;

    /// <summary>
    /// Physical name of the target field (column or JSON property) that is mutated in this scenario.<br/>
    /// It corresponds to the column/property name from the specification and is used to identify the field<br/>
    /// within the generated dataset where the scenario mutation should be applied.
    /// </summary>
    public string TargetField { get; } = targetField;

    /// <summary>
    /// Describes how the target field value is materialized for this scenario,<br/>
    /// which determines the specific mutation applied to the field in the generated dataset.
    /// </summary>
    public ScenarioBodyKind BodyKind { get; } = bodyKind;

    /// <summary>
    /// Optional literal used when <see cref="ScenarioBodyKind.Literal"/> is set.
    /// </summary>
    public string? Literal { get; } = literal;
}
using Whatever.TestData.Generator.Validation.Abstractions;

namespace Whatever.TestData.Generator.Validation.Scenarios;

/// <summary>
/// Builds scenario descriptors for a single field in a logical file.
/// </summary>
public interface IScenarioPlanner
{
    /// <summary>
    /// Enumerates all scenarios that should be generated for the given field.
    /// </summary>
    IReadOnlyList<TestScenarioDescriptor> Plan(
        FileSpecification file,
        FieldSpecification field,
        ScenarioSurfaceKind surface);
}
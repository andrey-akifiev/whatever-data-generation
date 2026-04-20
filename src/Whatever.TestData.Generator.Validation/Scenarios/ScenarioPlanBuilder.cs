using Whatever.TestData.Generator.Validation.Abstractions;

namespace Whatever.TestData.Generator.Validation.Scenarios;

/// <summary>
/// Collects specifications and exposes a fluent API for building scenario lists.
/// </summary>
public sealed class ScenarioPlanBuilder
{
    private readonly List<FileSpecification> _specifications = new();
    private ScenarioSurfaceKind _surface = ScenarioSurfaceKind.Tabular;

    /// <summary>
    /// Adds a logical file specification to the combined plan.
    /// </summary>
    public ScenarioPlanBuilder AddSpecification(FileSpecification specification)
    {
        ArgumentNullException.ThrowIfNull(specification);
        _specifications.Add(specification);
        return this;
    }

    /// <summary>
    /// Configures how NULL tokens are treated when generating scenarios.
    /// </summary>
    public ScenarioPlanBuilder WithSurface(ScenarioSurfaceKind surface)
    {
        _surface = surface;
        return this;
    }

    /// <summary>
    /// Expands every field in every specification into concrete scenarios using the supplied planner.
    /// </summary>
    public IReadOnlyList<TestScenarioDescriptor> BuildAll(IScenarioPlanner planner)
    {
        ArgumentNullException.ThrowIfNull(planner);
        if (_specifications.Count == 0)
        {
            throw new InvalidOperationException("At least one specification must be added.");
        }

        List<TestScenarioDescriptor> scenarios = new List<TestScenarioDescriptor>();
        foreach (FileSpecification specification in _specifications)
        {
            foreach (FieldSpecification field in specification.Fields)
                scenarios.AddRange(planner.Plan(specification, field, _surface));
        }

        return scenarios;
    }
}
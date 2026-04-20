using Microsoft.Extensions.DependencyInjection;

using Whatever.TestData.Generator.Validation.Output;
using Whatever.TestData.Generator.Validation.Scenarios;

namespace Whatever.TestData.Generator.Validation.Extensions;

/// <summary>
/// Registers validation generator services with <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds scenario planning, dataset writers, and orchestration services.
    /// </summary>
    public static IServiceCollection AddValidationGenerator(this IServiceCollection services)
    {
        services.AddSingleton<IScenarioPlanner, ScenarioPlanner>();

        services.AddSingleton<IDatasetWriterFactory, DatasetWriterFactory>();

        services.AddSingleton<SpecificationSetLoader>();
        services.AddSingleton<TestDataGenerator>();
        return services;
    }
}
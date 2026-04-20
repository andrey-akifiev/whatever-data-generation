using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;

using Whatever.TestData.Generator.Validation.Abstractions;
using Whatever.TestData.Generator.Validation.Output;
using Whatever.TestData.Generator.Validation.Scenarios;

namespace Whatever.TestData.Generator.Validation;

/// <summary>
/// Coordinates scenario expansion, dataset composition, and persistence to disk.
/// </summary>
public sealed class TestDataGenerator(
    IDatasetWriterFactory writerFactory,
    IScenarioPlanner planner,
    ILogger<TestDataGenerator>? logger)
{
    /// <summary>
    /// Generates all scenario folders, datasets, and summary CSV files under <paramref name="outputRoot"/>.
    /// </summary>
    public async Task GenerateAsync(
        string outputRoot,
        IReadOnlyList<FileSpecification> specifications,
        GeneratedDataFormat format,
        ScenarioSurfaceKind surface,
        DataWriterOptions writerOptions,
        CancellationToken cancellationToken = default,
        int maxDegreeOfParallelism = 4)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputRoot);
        ArgumentNullException.ThrowIfNull(specifications);
        if (specifications.Count == 0)
        {
            throw new ArgumentException("At least one specification is required.", nameof(specifications));
        }

        Directory.CreateDirectory(outputRoot);

        ScenarioPlanBuilder planBuilder = new ScenarioPlanBuilder().WithSurface(surface);
        foreach (FileSpecification specification in specifications)
        {
            planBuilder.AddSpecification(specification);
        }

        IReadOnlyList<TestScenarioDescriptor> scenarios = planBuilder.BuildAll(planner);
        logger?.LogInformation("Generated {Count} scenario descriptors", scenarios.Count);

        ConcurrentBag<string> positives = [];
        ConcurrentBag<string> negatives = [];

        ParallelOptions parallelOptions = new ()
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = maxDegreeOfParallelism,
        };

        await Parallel
            .ForEachAsync(scenarios, parallelOptions, async (scenario, token) =>
            {
                if (scenario.IsPositive)
                    positives.Add(scenario.FolderName);
                else
                    negatives.Add(scenario.FolderName);

                string targetDir = Path.Combine(outputRoot, scenario.FolderName);
                Directory.CreateDirectory(targetDir);

                IReadOnlyDictionary<string, IReadOnlyDictionary<string, string?>> matrix =
                    ScenarioMatrixBuilder.Build(specifications, scenario);
                IFileDatasetWriter writer = writerFactory.CreateWriter(format);

                foreach (FileSpecification specification in specifications)
                {
                    token.ThrowIfCancellationRequested();
                    IReadOnlyDictionary<string, string?> row = matrix[specification.LogicalName];
                    string fileName = FileNameBuilder.Build(specification.LogicalName, format, writerOptions);
                    string filePath = Path.Combine(targetDir, fileName);
                    await writer.WriteAsync(filePath, specification, row, token).ConfigureAwait(false);
                }

                logger?.LogDebug("Materialised scenario {Scenario}", scenario.FolderName);
            })
            .ConfigureAwait(false);

        await WriteIndexAsync(
            Path.Combine(outputRoot, "positive.csv"),
            positives.Order(StringComparer.Ordinal),
            token: cancellationToken).ConfigureAwait(false);
        await WriteIndexAsync(
            Path.Combine(outputRoot, "negative.csv"),
            negatives.Order(StringComparer.Ordinal),
            token: cancellationToken).ConfigureAwait(false);
        logger?.LogInformation("Finished writing datasets to {Root}", outputRoot);
    }

    private static async Task WriteIndexAsync(string path, IEnumerable<string> rows, CancellationToken token)
    {
        await using FileStream stream = File.Create(path);
        await using StreamWriter writer = new (stream);
        await writer.WriteLineAsync("TestScenario").ConfigureAwait(false);
        foreach (string row in rows)
        {
            await writer.WriteLineAsync(row).ConfigureAwait(false);
        }
    }
}
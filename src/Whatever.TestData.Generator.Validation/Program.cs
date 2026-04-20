using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Whatever.TestData.Generator.Validation.Abstractions;
using Whatever.TestData.Generator.Validation.Extensions;
using Whatever.TestData.Generator.Validation.Output;
using Whatever.TestData.Generator.Validation.Scenarios;
using Whatever.TestData.Generator.Validation.Specifications.Readers;

namespace Whatever.TestData.Generator.Validation;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        CommandLineOverrides cliOverrides;
        try
        {
            cliOverrides = ParseCommandLine(args);
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine(ex.Message);
            return PrintUsage();
        }

        IConfigurationRoot configuration = BuildConfiguration(cliOverrides.SettingsPath);
        GeneratorRuntimeOptions runtimeOptions = BindOptions(configuration);
        ApplyOverrides(runtimeOptions, cliOverrides);

        if (runtimeOptions.SpecificationPaths.Count == 0 || string.IsNullOrWhiteSpace(runtimeOptions.OutputRoot))
        {
            return PrintUsage();
        }

        if (!Enum.TryParse(runtimeOptions.Format, ignoreCase: true, out GeneratedDataFormat format))
        {
            Console.Error.WriteLine($"Unsupported format '{runtimeOptions.Format}'.");
            return 1;
        }

        ServiceCollection services = new ();
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSimpleConsole(options =>
            {
                options.TimestampFormat = "HH:mm:ss ";
                options.SingleLine = true;
            });
            builder.SetMinimumLevel(LogLevel.Information);
        });

        services.AddValidationGenerator();
        await using ServiceProvider provider = services.BuildServiceProvider();
        ILogger logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("validation-generator");

        try
        {
            SpecificationSetLoader loader = provider.GetRequiredService<SpecificationSetLoader>();
            TestDataGenerator generator = provider.GetRequiredService<TestDataGenerator>();

            SpecificationReaderOptions readerOptions = new SpecificationReaderOptions
            {
                SheetFilter = runtimeOptions.Sheets.Count == 0 ? null : runtimeOptions.Sheets,
                ColumnMapping = runtimeOptions.ColumnMapping,
            };

            logger.LogInformation("Loading {Count} specification files", runtimeOptions.SpecificationPaths.Count);
            IReadOnlyList<FileSpecification> specifications = await loader.LoadAsync(runtimeOptions.SpecificationPaths, readerOptions).ConfigureAwait(false);

            ScenarioSurfaceKind surface = format switch
            {
                GeneratedDataFormat.Csv or GeneratedDataFormat.Xlsx => ScenarioSurfaceKind.Tabular,
                GeneratedDataFormat.Json or GeneratedDataFormat.Yaml => ScenarioSurfaceKind.Structured,
                GeneratedDataFormat.Parquet => ScenarioSurfaceKind.ColumnarNullable,
                _ => ScenarioSurfaceKind.Tabular,
            };

            DataWriterOptions writerOptions = new DataWriterOptions { FilePrefix = runtimeOptions.Prefix };

            await generator
                .GenerateAsync(
                    runtimeOptions.OutputRoot,
                    specifications,
                    format,
                    surface,
                    writerOptions,
                    maxDegreeOfParallelism: runtimeOptions.MaxDegreeOfParallelism)
                .ConfigureAwait(false);

            logger.LogInformation("Generation completed successfully.");
            return 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Generation failed.");
            return 2;
        }
    }

    private static IConfigurationRoot BuildConfiguration(string? settingsPath)
    {
        IConfigurationBuilder builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);

        if (!string.IsNullOrWhiteSpace(settingsPath))
        {
            builder.AddJsonFile(settingsPath, optional: false, reloadOnChange: false);
        }

        return builder.Build();
    }

    private static GeneratorRuntimeOptions BindOptions(IConfigurationRoot configuration)
    {
        GeneratorRuntimeOptions options = new GeneratorRuntimeOptions();
        configuration.GetSection("ValidationGenerator").Bind(options);

        if (options.ColumnMapping is null)
        {
            options.ColumnMapping = SpecificationColumnMapping.Default;
        }

        options.SpecificationPaths ??= new List<string>();
        options.Sheets ??= new List<string>();
        options.Format ??= "csv";

        return options;
    }

    private static void ApplyOverrides(GeneratorRuntimeOptions runtimeOptions, CommandLineOverrides cliOverrides)
    {
        if (cliOverrides.SpecificationPaths.Count > 0)
        {
            runtimeOptions.SpecificationPaths = cliOverrides.SpecificationPaths;
        }

        if (!string.IsNullOrWhiteSpace(cliOverrides.OutputRoot))
        {
            runtimeOptions.OutputRoot = cliOverrides.OutputRoot;
        }

        if (!string.IsNullOrWhiteSpace(cliOverrides.Format))
        {
            runtimeOptions.Format = cliOverrides.Format;
        }

        if (cliOverrides.PrefixWasProvided)
        {
            runtimeOptions.Prefix = cliOverrides.Prefix;
        }

        if (cliOverrides.Sheets.Count > 0)
        {
            runtimeOptions.Sheets = cliOverrides.Sheets;
        }

        if (cliOverrides.MaxDegreeOfParallelism.HasValue)
        {
            runtimeOptions.MaxDegreeOfParallelism = cliOverrides.MaxDegreeOfParallelism.Value;
        }
    }

    private static CommandLineOverrides ParseCommandLine(string[] args)
    {
        CommandLineOverrides overrides = new CommandLineOverrides();

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--spec":
                case "-s":
                    EnsureValue(args, i);
                    overrides.SpecificationPaths.Add(args[++i]);
                    break;
                case "--out":
                case "-o":
                    EnsureValue(args, i);
                    overrides.OutputRoot = args[++i];
                    break;
                case "--format":
                case "-f":
                    EnsureValue(args, i);
                    overrides.Format = args[++i];
                    break;
                case "--prefix":
                case "-p":
                    EnsureValue(args, i);
                    overrides.Prefix = args[++i];
                    overrides.PrefixWasProvided = true;
                    break;
                case "--sheet":
                    EnsureValue(args, i);
                    overrides.Sheets.Add(args[++i]);
                    break;
                case "--settings":
                    EnsureValue(args, i);
                    overrides.SettingsPath = args[++i];
                    break;
                case "--maxdop":
                    EnsureValue(args, i);
                    overrides.MaxDegreeOfParallelism = int.Parse(args[++i]);
                    break;
                case "--help":
                case "-h":
                    PrintUsage();
                    Environment.Exit(0);
                    break;
                default:
                    Console.Error.WriteLine($"Unknown argument '{args[i]}'.");
                    throw new ArgumentException("Invalid CLI argument.");
            }
        }

        return overrides;
    }

    private static void EnsureValue(string[] args, int currentIndex)
    {
        if (currentIndex + 1 >= args.Length)
        {
            throw new ArgumentException($"Expected value after '{args[currentIndex]}'.");
        }
    }

    private static int PrintUsage()
    {
        Console.Error.WriteLine(
            """
            validation-generator

            Usage:
              validation-generator [--settings path/to/appsettings.json] [--spec <path> ...] [--out <dir>] [--format csv|xlsx|json|yaml|parquet] [--prefix name] [--sheet <name> ...] [--maxdop N]

            Priority:
              CLI arguments override values loaded from appsettings.json.

            appsettings.json section:
              ValidationGenerator

            Options:
              --settings    Optional path to additional appsettings JSON.
              --spec, -s    Path to a specification file (.csv, .xlsx, or .json). Repeat for multiple files.
              --out, -o     Output directory for generated scenarios.
              --format, -f  Physical format for generated datasets.
              --prefix, -p  Optional prefix applied to every generated file name.
              --sheet       Restrict XLSX import to listed worksheet names.
              --maxdop      Max degree of parallelism for scenario generation.
            """);

        return 1;
    }
}

public sealed class GeneratorRuntimeOptions
{
    public List<string> SpecificationPaths { get; set; } = new List<string>();

    public string? OutputRoot { get; set; }

    public string? Format { get; set; } = "csv";

    public string? Prefix { get; set; }

    public List<string> Sheets { get; set; } = new List<string>();

    public int MaxDegreeOfParallelism { get; set; } = 4;

    public SpecificationColumnMapping ColumnMapping { get; set; } = SpecificationColumnMapping.Default;
}

public sealed class CommandLineOverrides
{
    public List<string> SpecificationPaths { get; } = new List<string>();

    public string? OutputRoot { get; set; }

    public string? Format { get; set; }

    public string? Prefix { get; set; }

    public bool PrefixWasProvided { get; set; }

    public List<string> Sheets { get; } = new List<string>();

    public string? SettingsPath { get; set; }

    public int? MaxDegreeOfParallelism { get; set; }
}

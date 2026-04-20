namespace Whatever.TestData.Generator.Validation.Output;

/// <summary>
/// Builds physical file names for generated datasets.
/// </summary>
internal static class FileNameBuilder
{
    public static string Build(string logicalName, GeneratedDataFormat format, DataWriterOptions options)
    {
        string extension = format switch
        {
            GeneratedDataFormat.Csv => ".csv",
            GeneratedDataFormat.Xlsx => ".xlsx",
            GeneratedDataFormat.Json => ".json",
            GeneratedDataFormat.Yaml => ".yml",
            GeneratedDataFormat.Parquet => ".parquet",
            _ => throw new ArgumentOutOfRangeException(nameof(format)),
        };

        string name = string.IsNullOrWhiteSpace(options.FilePrefix)
            ? logicalName
            : $"{options.FilePrefix}_{logicalName}";

        return $"{name}{extension}";
    }
}
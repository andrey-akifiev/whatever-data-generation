using Microsoft.Extensions.Logging;

namespace Whatever.TestData.Generator.Validation.Output;

/// <inheritdoc />
public sealed class DatasetWriterFactory(ILoggerFactory? loggerFactory) : IDatasetWriterFactory
{
    /// <inheritdoc />
    public IFileDatasetWriter CreateWriter(GeneratedDataFormat format) =>
        format switch
        {
            GeneratedDataFormat.Csv => new CsvDatasetWriter(loggerFactory?.CreateLogger<CsvDatasetWriter>()),
            GeneratedDataFormat.Xlsx => new XlsxDatasetWriter(loggerFactory?.CreateLogger<XlsxDatasetWriter>()),
            GeneratedDataFormat.Json => new JsonDatasetWriter(loggerFactory?.CreateLogger<JsonDatasetWriter>()),
            GeneratedDataFormat.Yaml => new YamlDatasetWriter(loggerFactory?.CreateLogger<YamlDatasetWriter>()),
            GeneratedDataFormat.Parquet => new ParquetDatasetWriter(loggerFactory?.CreateLogger<ParquetDatasetWriter>()),
            _ => throw new ArgumentOutOfRangeException(nameof(format)),
        };
}
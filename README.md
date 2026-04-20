# ValidationGenerator

ValidationGenerator is a .NET-based tool that generates **test data scenario packs** from declarative specifications.

It is designed for validation testing where you need large, systematic sets of positive/negative datasets based on:

- field requiredness,
- SQL-like types,
- allowed ranges,
- nullability rules,
- and cross-file consistency.

A single run can produce many scenario directories, each containing one or more data files (`csv`, `xlsx`, `json`, `yml`, `parquet`) plus `positive.csv` and `negative.csv` indexes for your test framework.

---

## Table of Contents

- [What This Tool Solves](#what-this-tool-solves)
- [Current Project Layout](#current-project-layout)
- [Core Concepts](#core-concepts)
- [Specification Formats](#specification-formats)
    - [CSV](#csv)
    - [XLSX](#xlsx)
    - [JSON](#json)
- [Column/Property Name Mapping](#columnproperty-name-mapping)
- [Range Syntax](#range-syntax)
- [Generated Scenario Logic](#generated-scenario-logic)
- [Output Formats](#output-formats)
- [Configuration via appsettings.json](#configuration-via-appsettingsjson)
- [CLI Arguments](#cli-arguments)
- [Configuration Precedence](#configuration-precedence)
- [Quick Start](#quick-start)
- [Detailed End-to-End Example](#detailed-end-to-end-example)
- [Build and Test](#build-and-test)
- [Troubleshooting](#troubleshooting)
- [Extending the Tool](#extending-the-tool)

---

## What This Tool Solves

Typical ETL/API validation tests require many combinations of valid and invalid payloads.

Instead of hand-crafting files, you define a **specification** for your fields and let ValidationGenerator produce:

- boundary-value scenarios,
- empty/whitespace scenarios,
- type mismatch scenarios,
- missing required field scenarios,
- positive/negative scenario indexes.

It also preserves multi-file context. If your logical test case needs `accounts` + `locations`, each scenario directory contains both files, with only the target field mutated and all other required fields filled with valid values.

---

## Current Project Layout

This repository is now intentionally simplified:

- `src/ValidationGenerator` - **single executable project** containing all runtime code
- `tests/ValidationGenerator.Specification.Tests` - specification parsing/reading tests
- `tests/ValidationGenerator.Scenarios.Tests` - scenario planning/matrix tests
- `tests/ValidationGenerator.Output.Tests` - writer tests
- `tests/ValidationGenerator.Hosting.Tests` - orchestration integration tests
- `samples` - example specification files

---

## Core Concepts

### 1) Logical file specification
A logical specification describes one generated output file (for example `accounts`).

### 2) Field specification
Each field has:

- `Name`
- `Required`
- `Type` (SQL-like)
- `Range` (optional)
- `DefaultValue` (currently reserved)
- `NullValue` (optional)

### 3) Scenario descriptor
A scenario defines:

- target file,
- target field,
- mutated value/body kind,
- positive/negative classification,
- folder name.

### 4) Scenario pack
Each scenario pack is a folder containing all required files for that scenario.

---

## Specification Formats

### CSV

One CSV file = one logical file specification.

Example:

```csv
Name,Required,Type,Range,DefaultValue,NullValue
abc,Y,int,"[3,20]",,
cba,Y,int,"[3,20]",,
```

### XLSX

One workbook can contain multiple sheet specifications.

- Each sheet = one logical specification.
- Use sheet filter to include only selected sheets.

### JSON

Supported JSON shapes:

1) Single object:

```json
{
  "logicalName": "accounts",
  "fields": [
    { "name": "abc", "required": true, "type": "int", "range": "[3,20]", "nullValue": false }
  ]
}
```

2) Array of objects:

```json
[
  {
    "logicalName": "accounts",
    "fields": [
      { "name": "abc", "required": 1, "type": "int" }
    ]
  },
  {
    "logicalName": "locations",
    "fields": [
      { "name": "xyz", "required": 0, "type": "nvarchar(10)" }
    ]
  }
]
```

---

## Column/Property Name Mapping

Different projects often use different naming conventions. ValidationGenerator supports explicit mapping via options.

Default mapping values are:

- `name`
- `required`
- `type`
- `range`
- `defaultValue`
- `nullValue`
- `logicalName`
- `fields`

You can override all of them in `appsettings.json` (`ValidationGenerator:ColumnMapping`) and/or in code.

Example custom mapping (CSV/XLSX + JSON):

```json
"ColumnMapping": {
  "Name": "FieldName",
  "Required": "Mandatory",
  "Type": "DataType",
  "Range": "Allowed",
  "DefaultValue": "Def",
  "NullValue": "CanBeNull",
  "LogicalName": "Dataset",
  "Fields": "Columns"
}
```

---

## Range Syntax

Supported range forms:

- Interval notation:
    - `[1,1000]`
    - `[1,1000)`
    - `[0,)`
    - `(,0]`
- Discrete sets:
    - `{0,1,2}`
    - `{A,B,C}`
- Explicit nullable range marker:
    - `{NULL},[1,1000]`

Behavior notes:

- If range is missing, type defaults apply.
- For strings, interval is interpreted as **string length bounds**.
- Null allowance is computed from both `NullValue` and `{NULL}` in range.

---

## Generated Scenario Logic

For each field, planner creates scenarios from groups such as:

- empty/whitespace cases,
- boundary cases from interval/discrete rules,
- wrong-type values,
- missed required field (`missed`) if field is required,
- optional null scenario when format supports it.

Scenario folder naming:

`<file>_<field>_<slug>`

Examples:

- `accounts_abc_empty`
- `accounts_abc_3`
- `accounts_abc_20`
- `accounts_abc_missed`

Each output root also includes:

- `positive.csv` (header `TestScenario`)
- `negative.csv` (header `TestScenario`)

---

## Output Formats

Supported generated data formats:

- CSV (`.csv`)
- XLSX (`.xlsx`)
- JSON (`.json`)
- YAML (`.yml`)
- Parquet (`.parquet`)

File naming:

- base: `<logicalName>.<ext>`
- with prefix: `<prefix>_<logicalName>.<ext>`

---

## Configuration via appsettings.json

The app reads configuration from the `ValidationGenerator` section.

Example full config:

```json
{
  "ValidationGenerator": {
    "SpecificationPaths": [
      "samples/accounts.csv",
      "samples/locations.csv"
    ],
    "OutputRoot": "artifacts/out",
    "Format": "csv",
    "Prefix": "run1",
    "Sheets": ["accounts", "locations"],
    "MaxDegreeOfParallelism": 4,
    "ColumnMapping": {
      "Name": "name",
      "Required": "required",
      "Type": "type",
      "Range": "range",
      "DefaultValue": "defaultValue",
      "NullValue": "nullValue",
      "LogicalName": "logicalName",
      "Fields": "fields"
    }
  }
}
```

Notes:

- `appsettings.json` in current working directory is loaded automatically (optional).
- You can load additional config via `--settings <path>`.

---

## CLI Arguments

Run:

```bash
dotnet run --project src/ValidationGenerator -- [options]
```

Supported options:

- `--settings <path>` - additional appsettings JSON file
- `--spec`, `-s <path>` - specification path (repeatable)
- `--out`, `-o <dir>` - output root
- `--format`, `-f <csv|xlsx|json|yaml|parquet>` - output format
- `--prefix`, `-p <value>` - output file prefix
- `--sheet <name>` - XLSX sheet filter (repeatable)
- `--maxdop <int>` - max degree of parallelism
- `--help`, `-h` - usage

---

## Configuration Precedence

Priority is:

1. default values in code
2. `appsettings.json`
3. optional `--settings` file
4. **CLI args (highest priority)**

So CLI is always the final override layer.

---

## Quick Start

1) Build:

```bash
dotnet build ValidationGenerator.sln
```

2) Generate from sample specs:

```bash
dotnet run --project src/ValidationGenerator -- \
  --spec samples/accounts.csv \
  --spec samples/locations.csv \
  --out artifacts/demo \
  --format csv
```

3) Inspect result:

- `artifacts/demo/positive.csv`
- `artifacts/demo/negative.csv`
- one folder per scenario

---

## Detailed End-to-End Example

Given:

`accounts.csv`

```csv
Name,Required,Type,Range,DefaultValue,NullValue
abc,Y,int,"[3,20]",,
cba,Y,int,"[3,20]",,
```

`locations.csv`

```csv
Name,Required,Type,Range,DefaultValue,NullValue
xyz,Y,int,"[3,20]",,
zyx,Y,int,"[3,20]",,
```

For scenario `accounts_abc_21` (negative), generated `accounts.csv` could contain:

```csv
abc,cba
21,3
```

and `locations.csv` remains valid:

```csv
xyz,zyx
3,3
```

`negative.csv` includes `accounts_abc_21`.

`positive.csv` includes valid boundary scenarios like `accounts_abc_3` and `accounts_abc_20`.

---

## Build and Test

Build:

```bash
dotnet build ValidationGenerator.sln
```

Test:

```bash
dotnet test ValidationGenerator.sln
```

Coverage (optional):

```bash
dotnet test ValidationGenerator.sln --collect:"XPlat Code Coverage"
```

---

## Troubleshooting

### Missing required inputs
If no `SpecificationPaths` or `OutputRoot` are provided after config+CLI merge, app prints usage and exits.

### Unsupported format
If `Format` value is not one of `csv|xlsx|json|yaml|parquet`, app exits with error.

### JSON key mismatch
If your JSON spec uses custom property names, ensure `ColumnMapping` is configured accordingly.

### XLSX sheet mismatch
If `Sheets` filter is set but no worksheet matches, loading fails.

---

## Extending the Tool

Common extension points:

- Add new specification reader implementation (`ISpecificationReader` pattern in Specification namespace).
- Add new output format by implementing `IFileDatasetWriter` and wiring `DatasetWriterFactory`.
- Add scenario heuristics in `ScenarioPlanner`.
- Add configuration options to `GeneratorRuntimeOptions` and bind from `ValidationGenerator` section.

---

## Notes

- The tool is asynchronous where practical and parallelized in scenario materialization.
- Logging uses `ILogger` and writes to console in CLI runtime.
- Design targets a robust reusable runtime with explicit config control for different projects.

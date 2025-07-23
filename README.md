# DotNetDaterbaser

DotNetDaterbaser is a small command line utility for applying SQL Server scripts as part of a .NET build. It manages execution order for full database scripts and incremental update scripts using a `tracking.json` file in the input directory. The program is intended to be run during a prebuild step so database changes stored in the repository are automatically applied.

## Usage

```
DotNetDaterbaser <connectionString1> [<connectionString2> ...] <outputDir> <scriptsDir>
```

* **connectionString** – one or more SQL Server connection strings.
* **outputDir** – directory used for log files.
* **scriptsDir** – directory containing SQL scripts and `tracking.json`.

The tool ensures `tracking.json`, `.gitignore`, and `AGENTS.md` exist in the scripts directory before running scripts.

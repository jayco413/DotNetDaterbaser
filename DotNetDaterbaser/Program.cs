using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

class TrackingEntry
{
    public bool FullRun { get; set; }
    public HashSet<string> Scripts { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

class Program
{
    static async Task<int> Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: DotNetDaterbaser <connectionString1> [<connectionString2> ...] <outputDir> <scriptsDir>");
            return 1;
        }

        var outputDir = args[^2];
        var inputDir = args[^1];
        var connectionStrings = args.Take(args.Length - 2).ToArray();

        Directory.CreateDirectory(outputDir);
        Directory.CreateDirectory(inputDir);

        var trackingPath = Path.Combine(inputDir, "tracking.json");
        Dictionary<string, TrackingEntry> tracking;
        if (!File.Exists(trackingPath))
        {
            tracking = new Dictionary<string, TrackingEntry>();
            await File.WriteAllTextAsync(trackingPath, JsonSerializer.Serialize(tracking, new JsonSerializerOptions { WriteIndented = true }));
        }
        else
        {
            try
            {
                var json = await File.ReadAllTextAsync(trackingPath);
                tracking = JsonSerializer.Deserialize<Dictionary<string, TrackingEntry>>(json) ?? new();
            }
            catch
            {
                tracking = new Dictionary<string, TrackingEntry>();
            }
        }

        var gitignorePath = Path.Combine(inputDir, ".gitignore");
        if (!File.Exists(gitignorePath))
        {
            await File.WriteAllTextAsync(gitignorePath, "tracking.json\n");
        }
        else
        {
            var lines = new HashSet<string>(await File.ReadAllLinesAsync(gitignorePath));
            if (!lines.Contains("tracking.json"))
            {
                await File.AppendAllTextAsync(gitignorePath, "\ntracking.json\n");
            }
        }

        var agentsPath = Path.Combine(inputDir, "AGENTS.md");
        if (!File.Exists(agentsPath))
        {
            var samplePath = Path.Combine(AppContext.BaseDirectory, "SampleAgents.txt");
            if (File.Exists(samplePath))
            {
                await File.WriteAllTextAsync(agentsPath, await File.ReadAllTextAsync(samplePath));
            }
        }

        foreach (var conn in connectionStrings)
        {
            var builder = new SqlConnectionStringBuilder(conn);
            var server = builder.DataSource.Replace("\\", "_").Replace("/", "_").Replace(":", "_");
            var database = builder.InitialCatalog;
            var key = $"{server}_{database}";
            if (!tracking.TryGetValue(key, out var entry))
            {
                entry = new TrackingEntry();
                tracking[key] = entry;
            }

            var prefix = $"{server}_{database}_";
            var fullFile = Path.Combine(inputDir, $"{prefix}full_database_script.sql");
            var partials = Directory.GetFiles(inputDir, $"{prefix}*_script.sql")
                .Where(f => !f.EndsWith("full_database_script.sql", StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f)
                .ToList();

            var logFile = Path.Combine(outputDir, $"{key}.log");

            if (!entry.FullRun && File.Exists(fullFile))
            {
                await RunSqlScriptAsync(conn, await File.ReadAllTextAsync(fullFile));
                entry.FullRun = true;
                foreach (var p in partials)
                    entry.Scripts.Add(Path.GetFileName(p));
                await File.AppendAllTextAsync(logFile, $"Ran full script {Path.GetFileName(fullFile)}{Environment.NewLine}");
            }
            else
            {
                foreach (var p in partials)
                {
                    var name = Path.GetFileName(p);
                    if (!entry.Scripts.Contains(name))
                    {
                        await RunSqlScriptAsync(conn, await File.ReadAllTextAsync(p));
                        entry.Scripts.Add(name);
                        await File.AppendAllTextAsync(logFile, $"Ran script {name}{Environment.NewLine}");
                    }
                }
            }
        }

        await File.WriteAllTextAsync(trackingPath, JsonSerializer.Serialize(tracking, new JsonSerializerOptions { WriteIndented = true }));
        return 0;
    }

    static async Task RunSqlScriptAsync(string connectionString, string sql)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        using var command = new SqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }
}

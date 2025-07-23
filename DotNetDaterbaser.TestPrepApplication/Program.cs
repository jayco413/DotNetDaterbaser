using Microsoft.Data.SqlClient;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DotNetDaterbaser.TestPrepApplication
{
    /// <summary>
    /// Prepares databases for tests by clearing existing tables and
    /// applying the Gamma full script and the first five partial scripts.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main entry point for the prep application.
        /// </summary>
        /// <returns>An awaitable task.</returns>
        public static async Task Main()
        {
            var alpha = "Server=localhost;Database=DotNetDaterbaserAlpha;Trusted_Connection=True;TrustServerCertificate=True";
            var beta = "Server=localhost;Database=DotNetDaterbaserBeta;Trusted_Connection=True;TrustServerCertificate=True";
            var gamma = "Server=localhost;Database=DotNetDaterbaserGamma;Trusted_Connection=True;TrustServerCertificate=True";

            var connections = new[] { alpha, beta, gamma };
            foreach (var cs in connections)
            {
                await DropTablesAsync(cs).ConfigureAwait(false);
            }

            var scriptsDir = Path.Combine(Environment.CurrentDirectory, "Scripts");
            var prefix = "localhost_DotNetDaterbaserGamma_";
            var full = Path.Combine(scriptsDir, $"{prefix}full_database_script.sql");
            var partials = Directory.GetFiles(scriptsDir, $"{prefix}*_script.sql")
                .Where(f => !f.EndsWith("full_database_script.sql", StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f)
                .Take(5);

            await RunSqlScriptAsync(gamma, await File.ReadAllTextAsync(full));
            foreach (var file in partials)
            {
                await RunSqlScriptAsync(gamma, await File.ReadAllTextAsync(file));
            }

            await UpdateTrackingAsync(gamma, scriptsDir, partials);
        }

        private static async Task DropTablesAsync(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            const string tableSql = "SELECT TABLE_SCHEMA, TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
            var tables = new List<(string Schema, string Name)>();

            await using (var cmd = new SqlCommand(tableSql, connection))
            await using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    tables.Add((reader.GetString(0), reader.GetString(1)));
                }
            }

            foreach (var (schema, name) in tables)
            {
                var sql = $"DROP TABLE [{schema}].[{name}]";
                using var dropCmd = new SqlCommand(sql, connection);
                await dropCmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        private static async Task RunSqlScriptAsync(string connectionString, string sql)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            foreach (var batch in SplitSqlBatches(sql))
            {
                using var command = new SqlCommand(batch, connection);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        private static IEnumerable<string> SplitSqlBatches(string sql)
        {
            var batches = Regex.Split(
                sql,
                @"^\s*GO\s*(?:\r?\n|$)",
                RegexOptions.Multiline | RegexOptions.IgnoreCase);

            return batches.Where(static batch => !string.IsNullOrWhiteSpace(batch));
        }

        private static async Task UpdateTrackingAsync(string connectionString, string scriptsDir, IEnumerable<string> partials)
        {
            var trackingPath = Path.Combine(scriptsDir, "tracking.json");
            Dictionary<string, TrackingEntry> tracking;

            if (File.Exists(trackingPath))
            {
                var json = await File.ReadAllTextAsync(trackingPath).ConfigureAwait(false);
                tracking = JsonSerializer.Deserialize<Dictionary<string, TrackingEntry>>(json) ?? new();
            }
            else
            {
                tracking = new Dictionary<string, TrackingEntry>();
            }

            var builder = new SqlConnectionStringBuilder(connectionString);
            var server = builder.DataSource.Replace("\\", "_").Replace("/", "_").Replace(":", "_");
            var key = $"{server}_{builder.InitialCatalog}";

            if (!tracking.TryGetValue(key, out var entry))
            {
                entry = new TrackingEntry();
                tracking[key] = entry;
            }

            entry.FullRun = true;
            foreach (var file in partials)
            {
                entry.Scripts.Add(Path.GetFileName(file));
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            await File.WriteAllTextAsync(trackingPath, JsonSerializer.Serialize(tracking, options)).ConfigureAwait(false);
        }
    }
}

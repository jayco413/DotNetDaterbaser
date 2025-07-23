using Microsoft.Data.SqlClient;

namespace DotNetDaterbaser.TestApplication
{
    /// <summary>
    /// Entry point for the test application.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Verifies required tables and columns exist in both databases.
        /// </summary>
        /// <returns>An awaitable task.</returns>
        public static async Task Main()
        {
            var databases = new[] { "DotNetDaterbaserAlpha", "DotNetDaterbaserBeta" };
            foreach (var db in databases)
            {
                var cs = $"Server=localhost;Database={db};Trusted_Connection=True;TrustServerCertificate=True";
                await VerifyTablesAsync(cs);
            }

            Console.WriteLine("Database verification succeeded.");
        }

        /// <summary>
        /// Checks that the expected table and columns exist.
        /// </summary>
        /// <param name="connectionString">The connection string to use.</param>
        /// <returns>An awaitable task.</returns>
        private static async Task VerifyTablesAsync(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            const string tableCheck = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TestTable'";
            using (var cmd = new SqlCommand(tableCheck, connection))
            {
                var count = (int)await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (count != 1)
                {
                    throw new InvalidOperationException($"Table TestTable missing in {connectionString}");
                }
            }

            var columns = new[] { "Id", "Name", "CreatedOn" };
            foreach (var column in columns)
            {
                var sql = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TestTable' AND COLUMN_NAME = @column";
                using var columnCmd = new SqlCommand(sql, connection);
                columnCmd.Parameters.AddWithValue("@column", column);
                var count = (int)await columnCmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (count != 1)
                {
                    throw new InvalidOperationException($"Column {column} missing in {connectionString}");
                }
            }
        }
    }
}

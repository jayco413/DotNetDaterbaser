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
            var alpha = "Server=localhost;Database=DotNetDaterbaserAlpha;Trusted_Connection=True;TrustServerCertificate=True";
            var beta = "Server=localhost;Database=DotNetDaterbaserBeta;Trusted_Connection=True;TrustServerCertificate=True";
            var gamma = "Server=localhost;Database=DotNetDaterbaserGamma;Trusted_Connection=True;TrustServerCertificate=True";

            await VerifyNoTablesAsync(alpha).ConfigureAwait(false);
            await VerifyTablesAsync(beta).ConfigureAwait(false);
            await VerifyGammaAsync(gamma).ConfigureAwait(false);

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

        private static async Task VerifyNoTablesAsync(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            const string tableCheck = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
            using var cmd = new SqlCommand(tableCheck, connection);
            var count = (int)await cmd.ExecuteScalarAsync().ConfigureAwait(false);
            if (count != 0)
            {
                throw new InvalidOperationException($"Expected no tables in {connectionString}");
            }
        }

        private static async Task VerifyGammaAsync(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            const string countRows = "SELECT COUNT(*) FROM TestTable";
            using var cmd = new SqlCommand(countRows, connection);
            var count = (int)await cmd.ExecuteScalarAsync().ConfigureAwait(false);
            if (count != 10)
            {
                throw new InvalidOperationException($"Expected 10 rows in TestTable for {connectionString}");
            }
        }

    }
}

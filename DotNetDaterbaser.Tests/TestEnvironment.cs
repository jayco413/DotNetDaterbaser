using System.Threading;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetDaterbaser.Tests
{
    public static class TestEnvironment
    {
        public const string ConnectionString = "Server=localhost,1433;Database=TestDb;User Id=sa;Password=YourStrong!Passw0rd;";
        private static bool _initialized;

        public static void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            if (!DockerHelper.StartContainer())
            {
                Assert.Inconclusive("Docker not available");
            }

            Thread.Sleep(TimeSpan.FromSeconds(10));
            CreateDatabase();
            _initialized = true;
        }

        public static void Cleanup()
        {
            DockerHelper.StopContainer();
        }

        private static void CreateDatabase()
        {
            var builder = new SqlConnectionStringBuilder(ConnectionString)
            {
                InitialCatalog = "master"
            };

            using var connection = new SqlConnection(builder.ConnectionString);
            connection.Open();
            using var command = new SqlCommand("IF DB_ID('TestDb') IS NULL CREATE DATABASE TestDb;", connection);
            command.ExecuteNonQuery();
        }
    }
}

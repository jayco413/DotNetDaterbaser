using System.Reflection;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetDaterbaser.Tests
{
    [TestClass]
    public sealed class SqlScriptTests
    {
        [TestMethod]
        public async Task RunSqlScriptAsyncCreatesTable()
        {
            var method = typeof(Program).GetMethod("RunSqlScriptAsync", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method);
            await (Task)method!.Invoke(null, new object[] { TestEnvironment.ConnectionString, "CREATE TABLE dbo.DirectTest(Id INT PRIMARY KEY);" })!;

            using var connection = new SqlConnection(TestEnvironment.ConnectionString);
            await connection.OpenAsync();
            using var command = new SqlCommand("SELECT 1 FROM sys.tables WHERE name='DirectTest'", connection);
            var result = await command.ExecuteScalarAsync();
            Assert.IsNotNull(result);
        }
    }
}

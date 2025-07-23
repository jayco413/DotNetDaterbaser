using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetDaterbaser.Tests
{
    [TestClass]
    public sealed class ProgramTests
    {
        [TestMethod]
        public async Task ProgramMainRunsScripts()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var outputDir = Path.Combine(tempDir, "out");
            var scriptsDir = Path.Combine(tempDir, "scripts");
            Directory.CreateDirectory(outputDir);
            Directory.CreateDirectory(scriptsDir);

            var fullScript = Path.Combine(scriptsDir, "localhost_TestDb_full_database_script.sql");
            await File.WriteAllTextAsync(fullScript, "CREATE TABLE dbo.Tests(Id INT PRIMARY KEY);");

            var exitCode = await Program.Main(new[] { TestEnvironment.ConnectionString, outputDir, scriptsDir });
            Assert.AreEqual(0, exitCode, "First run failed");

            var incScript = Path.Combine(scriptsDir, "localhost_TestDb_001_script.sql");
            await File.WriteAllTextAsync(incScript, "INSERT INTO dbo.Tests(Id) VALUES(1);");

            exitCode = await Program.Main(new[] { TestEnvironment.ConnectionString, outputDir, scriptsDir });
            Assert.AreEqual(0, exitCode, "Second run failed");

            var trackingPath = Path.Combine(scriptsDir, "tracking.json");
            Assert.IsTrue(File.Exists(trackingPath));
            var json = await File.ReadAllTextAsync(trackingPath);
            var tracking = JsonSerializer.Deserialize<Dictionary<string, TrackingEntry>>(json);
            Assert.IsNotNull(tracking);
            Assert.IsTrue(tracking!.Count > 0);
        }
    }
}

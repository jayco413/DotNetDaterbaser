using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetDaterbaser.Tests
{
    public static class DockerHelper
    {
        public static bool StartContainer()
        {
            return Run("docker", "run -e \"ACCEPT_EULA=Y\" -e \"SA_PASSWORD=YourStrong!Passw0rd\" -p 1433:1433 --name test-sqlserver -d mcr.microsoft.com/mssql/server:2022-latest") == 0;
        }

        public static void StopContainer()
        {
            Run("docker", "stop test-sqlserver");
            Run("docker", "rm test-sqlserver");
        }

        private static int Run(string fileName, string arguments)
        {
            var startInfo = new ProcessStartInfo(fileName, arguments)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            try
            {
                using var process = Process.Start(startInfo);
                process!.WaitForExit();
                return process.ExitCode;
            }
            catch
            {
                return -1;
            }
        }
    }
}

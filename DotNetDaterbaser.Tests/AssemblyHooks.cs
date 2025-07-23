using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetDaterbaser.Tests
{
    [TestClass]
    public static class AssemblyHooks
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            TestEnvironment.Initialize();
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            TestEnvironment.Cleanup();
        }
    }
}

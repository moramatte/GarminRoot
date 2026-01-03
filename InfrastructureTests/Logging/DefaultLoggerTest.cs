using System;
using System.IO;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Extensions;
using Infrastructure.Logger;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Infrastructure.GlobalFor;

namespace InfrastructureTests.Logging
{
	[TestClass]
	public class DefaultLoggerTest
	{
        [TestInitialize]
        public void Setup()
        {
            ServiceLocator.Reset();
        }

        private string LogPath => Path.Combine(ConfigFile.Folder, "SimpleLog.log");

        [TestMethod]
        public async Task LogToDiskIsProduced()
        {
            ServiceLocator.LogToSimpleFile(LogPath);

            var randomToken = Chance.Within(0, 99999);
            Log.Info($"Logging Token: {randomToken}");

            await Task.Delay(For(1).Seconds());

            Assert.IsTrue(File.Exists(LogPath));
            File.Copy(LogPath, $"{LogPath}Copy", true);

			var logContent = File.ReadAllText($"{LogPath}Copy");
			Assertions.AssertContains(logContent, $"Token: {randomToken}");
		}

        [TestMethod]
        [TestCategory(TestCategories.LocalOnly)]
        public async Task LogToDiskIsProducedWithUnrootedPath()
        {
            var unrootedPath = "ConfiguratorLogs\\AwosLog.log";
            var rootedPath = Path.Combine(ConfigFile.Folder, "Logs\\ConfiguratorLogs\\AwosLog.log");
            ServiceLocator.LogToSimpleFile(unrootedPath);

            var randomToken = Chance.Within(0, 99999);
            Log.Info($"Logging Token: {randomToken}");

            await Task.Delay(For(1).Seconds());

            Assert.IsTrue(File.Exists(rootedPath));
            IO.Copy(LogPath, $"{rootedPath}Copy", true);

            var logContent = IO.ReadAllText($"{rootedPath}Copy");
            Assertions.AssertContains(logContent, $"Token: {randomToken}");
        }

        [TestMethod]
        public async Task FlushCanBeCalled()
        {
			ServiceLocator.LogToSimpleFile(LogPath);

            var randomToken = Chance.Within(0, 99999);
            Log.Info($"Logging Token: {randomToken}");
            Log.Flush();

            await Task.Delay(For(1).Seconds());

            Assert.IsTrue(File.Exists(LogPath));
            File.Copy(LogPath, $"{LogPath}Copy", true);

            var logContent = File.ReadAllText($"{LogPath}Copy");
            Assertions.AssertContains(logContent, $"Token: {randomToken}");
            Assertions.AssertLacks(logContent, $"Flush failed");
		}
	}
}

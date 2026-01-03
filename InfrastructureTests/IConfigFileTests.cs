using System.IO.Abstractions;
using Infrastructure;
using Infrastructure.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InfrastructureTests
{
    [TestClass]
    public class IConfigFileTests
    {
        private string fileLocation = @"C:\System\Awos\SystemConfig.xml";
        private IFileSystem IO;

        [TestInitialize]
        public void Setup()
        {
            IO = InMyTest.UseFakeFileFystem();
            IO.Directory.CreateDirectory(@"C:\System\Awos");
        }

        [TestMethod]
        public void ConfigFileIsAutomaticallyMigrated()
        {
            var configFile = new MyConfigFile();
            configFile.ToXmlConfigFile(fileLocation);

            var onDiskRaw = IO.File.ReadAllText(fileLocation);
            var gotBack = onDiskRaw.FromXml<MyConfigFile>();
            Assert.IsNull(gotBack.Info);
            Assert.AreEqual(0, gotBack.Version);

            gotBack = fileLocation.FromXmlConfigFile<MyConfigFile>();
            Assert.AreEqual("Migrated", gotBack.Info);
            Assert.AreEqual(1, gotBack.Version);
        }
    }
  
    public class MyConfigFile : IVersionableConfigFile
    {
        public int LatestVersion => 1;
        public int Version { get; set; }
        public string Info { get; set; }
        public void Migrate()
        {
            Info = "Migrated";
        }
    }
}

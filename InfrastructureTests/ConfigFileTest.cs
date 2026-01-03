using System;
using System.ComponentModel;
using System.IO;
using Infrastructure;
using Infrastructure.Extensions;
using Infrastructure.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InfrastructureTests
{
    public class ConfigFileSettings
    {
        public bool Activated { get; set; }

        [DefaultValue("James")]
        public string Name { get; set; }
    }

    public class ComplexSettings : IPopulateDefaultValues
    {
        public ConfigFileSettings Complex { get; set; }
        public void PopulateDefaultValues()
        {
            Complex = new ConfigFileSettings() { Name = "ComplexMan" };
        }
    }

    public class IConfigFileImplementation : IVersionableConfigFile, IPopulateDefaultValues
    {
        public string Name { get; set; }
        public int LatestVersion => 1;
        public int Version { get; set; }
        public void Migrate()
        {
            PopulateDefaultValues();
        }

        public void PopulateDefaultValues()
        {
            if (Version < LatestVersion)
            {
                Name = "Joe";
                Version = LatestVersion;
            }
        }
    }

    [TestClass]
	public class ConfigFileTest
    {
        private string F(string fileName) => Path.Combine(ConfigFile.Folder, fileName);

        [TestMethod]
        public void SettingsArePersisted()
        {
            InMyTest.UseFakeFileFystem();

            Assert.IsFalse(IO.FileExists(F("ConfigFileSettings.json")));
            var config = ConfigFile.Get<ConfigFileSettings>();
            Assert.AreEqual("James", config.Name);

            config.Name = "Ren";
            config.Activated = true;
            ConfigFile.Save(config);

            Assert.IsTrue(IO.FileExists(F("ConfigFileSettings.json")));
            var newConfig = ConfigFile.Get<ConfigFileSettings>();
            Assert.IsTrue(newConfig.Activated);
            Assert.AreEqual("Ren", newConfig.Name);
        }

        [TestMethod]
        public void ComplexMembersArePopulated()
        {
            InMyTest.UseFakeFileFystem();

            Assert.IsFalse(IO.FileExists(F("ComplexSettings.json")));
            var config = ConfigFile.Get<ComplexSettings>();
            Assert.AreEqual("ComplexMan", config.Complex.Name);
        }

        [TestMethod]
        public void IConfigFilesAreMigrated()
        {
            InMyTest.UseFakeFileFystem();

            var filePath = F("IConfigFileImplementation.json");

			Assert.IsFalse(IO.FileExists(filePath));
            var config = new IConfigFileImplementation();
            Assert.IsNull(config.Name);
            IO.WriteAllText(filePath, config.ToJson());

            var reloadedConfig = ConfigFile.Get<IConfigFileImplementation>();
            Assert.AreEqual(1, reloadedConfig.Version);
            Assert.AreEqual("Joe", reloadedConfig.Name);
        }
	}
}

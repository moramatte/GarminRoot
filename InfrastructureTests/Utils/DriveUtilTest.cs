using System.IO;
using Infrastructure;
using Infrastructure.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InfrastructureTests.Utils
{
	[TestClass]
    [TestCategory(TestCategories.Ignored)]
	public class DriveUtilTest
	{
        [TestInitialize]
        public void Setup()
        {
            DriveUtil.Disconnect("B:");
        }

		[TestMethod]
        public void BCanBeMounted()
        {
            Assert.IsFalse(Directory.Exists(@"B:\devops"));

            Mount.Drive("B:", @"\\dev.awos.met\share", "administrator", "awos2023!!!");
            Assert.IsTrue(Directory.Exists(@"B:\devops"));

            DriveUtil.Disconnect("B:");
            Assert.IsFalse(Directory.Exists(@"B:\devops"));
		}
	}
}

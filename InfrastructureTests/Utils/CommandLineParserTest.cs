using Infrastructure.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InfrastructureTests.Utils
{
    [TestClass]
    public class CommandLineParserTest
    {
        public enum CLPEnum
        {
            One, Two, Three
        }

        [TestMethod]
        public void HasFlagWorks()
        {
            Assert.IsFalse(CommandLineParser.HasFlag("bla bla", CLPEnum.One));
            Assert.IsTrue(CommandLineParser.HasFlag("bla One bla", CLPEnum.One));

            Assert.IsFalse(CommandLineParser.HasFlag(["bla","bla"], CLPEnum.One));
            Assert.IsTrue(CommandLineParser.HasFlag(["bla","One","bla"], CLPEnum.One));
        }

        [TestMethod]
        public void GetSwitchWorks()
        {
            Assert.AreEqual("SAAB", CommandLineParser.GetSwitch("bla Two SAAB bla", CLPEnum.Two));
            Assert.AreEqual("SAAB", CommandLineParser.GetSwitch(["bla","Two","SAAB","bla"], CLPEnum.Two));
            Assert.AreEqual("SAAB", CommandLineParser.GetSwitch("bla -Two SAAB bla", CLPEnum.Two));
            Assert.AreEqual("SAAB", CommandLineParser.GetSwitch("bla --Two SAAB bla", CLPEnum.Two));
            Assert.AreEqual("SAAB", CommandLineParser.GetSwitch("bla /Two SAAB bla", CLPEnum.Two));
        }
    }
}

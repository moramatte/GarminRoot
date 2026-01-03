using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Infrastructure;
using Infrastructure.Network;
using Infrastructure.Utilities;
using InfrastructureTests.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InfrastructureTests
{
	[TestClass]
    public class UtilitiesTest
    {
        [TestMethod]
        public void ThrowWorksGivenPredicate()
        {
            Func<bool> predicate = () => false;
            Exception ex = null;

            Throw.If(predicate, "Predicate failed");

            predicate = () => true;
            try
            {
                Throw.If(predicate, "Predicate failed");
            }
            catch (Exception e)
            {
                ex = e;
            }

            Assert.IsNotNull(ex);
            Assert.AreEqual("Predicate failed", ex.Message);
        }

        [TestMethod]
        public void ChanceWorksWithSeed()
        {
            var val1 = Chance.Within(0, 100000);
            var val2 = Chance.Within(0, 100000);
            Assert.AreNotEqual(val1, val2);

            var seed = (int)new DateTime(DateTime.Now.Day).Ticks;
            val1 = Chance.Within(seed, 0, 100000);
            val2 = new Random(seed).Next(0, 100000);
            Assert.AreEqual(val1, val2);
        }

        [TestMethod]
        public void ThrowWorksGivenSwitch()
        {
            bool condition = false;
            Exception ex = null;

            Throw.If(condition, "Predicate failed");

            condition = true;
            try
            {
                Throw.If(condition, "Predicate failed");
            }
            catch (Exception e)
            {
                ex = e;
            }

            Assert.IsNotNull(ex);
            Assert.AreEqual("Predicate failed", ex.Message);
        }

        [TestMethod]
        public void ThrowWorksWithTypeParameter()
        {
            Func<bool> predicate = () => false;
            Exception ex = null;

            Throw<InvalidOperationException>.If(predicate, "Predicate failed");

            predicate = () => true;
            try
            {
                Throw<InvalidOperationException>.If(predicate, "Predicate failed");
            }
            catch (Exception e)
            {
                ex = e;
            }

            Assert.IsNotNull(ex);
            Assert.IsTrue(ex is InvalidOperationException);
            Assert.AreEqual("Predicate failed", ex.Message);
        }

        [TestMethod]
        public void ThrowWorksWithTypeParameterAndSwitch()
        {
            bool condition = false;
            Exception ex = null;

            Throw<InvalidOperationException>.If(condition, "Predicate failed");

            condition = true;
            try
            {
                Throw<InvalidOperationException>.If(condition, "Predicate failed");
            }
            catch (Exception e)
            {
                ex = e;
            }

            Assert.IsNotNull(ex);
            Assert.IsTrue(ex is InvalidOperationException);
            Assert.AreEqual("Predicate failed", ex.Message);
        }

        [TestMethod]
        public void DnsWrapperCanProvideHostEntry()
        {
            var result = new DnsWrapper().GetHostEntryAsync("localhost").Result;
            Assert.IsTrue(result.AddressList.Length > 0);
        }
    }
}

using FakeItEasy;
using Infrastructure;
using Infrastructure.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace InfrastructureTests
{
    [TestClass]
    public class TimedCacheTests
	{
        [TestMethod]
        public async Task When_timespan_elapsed_HasValue_is_false()
        {
            const string val = "Hello";
            var tCache = new TimedCache<string>(TimeSpan.FromMilliseconds(300));
            Assert.IsFalse(tCache.HasValue);
            tCache.SetValue(val);
			Assert.IsTrue(tCache.HasValue);
            Assert.AreEqual(val, tCache.Value);
            await Task.Delay(400);
			Assert.IsFalse(tCache.HasValue);
		}

		[TestMethod]
		public async Task When_timespan_elapsed_value_is_unchanged()
		{
			const int val = 5;
			var tCache = new TimedCache<int>(TimeSpan.FromMilliseconds(300));
			Assert.IsFalse(tCache.HasValue);
			tCache.SetValue(val);
			Assert.IsTrue(tCache.HasValue);
			Assert.AreEqual(val, tCache.Value);
			await Task.Delay(400);
			Assert.AreEqual(val, tCache.Value);
			Assert.IsFalse(tCache.HasValue);
		}
	}
}
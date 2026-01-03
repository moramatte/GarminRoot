using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Infrastructure.Extensions;

namespace InfrastructureTests.Utils
{
	[TestClass]
	public class DateTimeLengthComparerTest
	{
		[TestMethod]
		public void IsOlder()
		{
			var now = DateTime.UtcNow;
			var then = now.AddMinutes(1);
			Assert.IsTrue(now.Is().OlderThan(then));
		}

		[TestMethod]
		public void IsMinOlder()
		{
			var now = DateTime.UtcNow;
			var then = now.AddMinutes(2);
			Assert.IsTrue(now.Is(1).Minutes().OlderThan(then));
			Assert.IsFalse(now.Is(3).Minutes().OlderThan(then));
		}

		[TestMethod]
		public void IsSecOlder()
		{
			var now = DateTime.UtcNow;
			var then = now.AddSeconds(20);
			Assert.IsTrue(now.Is(10).Seconds().OlderThan(then));
			Assert.IsFalse(now.Is(30).Seconds().OlderThan(then));
		}

		[TestMethod]
		public void IsHoursOlder()
		{
			var now = DateTime.UtcNow;
			var then = now.AddHours(20);
			Assert.IsTrue(now.Is(10).Hours().OlderThan(then));
			Assert.IsFalse(now.Is(30).Hours().OlderThan(then));
		}

		[TestMethod]
		public void IsNotOlder()
		{
			var now = DateTime.UtcNow;
			var then = now.AddMinutes(1);
			Assert.IsFalse(now.Is().Not().OlderThan(then));
		}

		[TestMethod]
		public void IsNotMinOlder()
		{
			var now = DateTime.UtcNow;
			var then = now.AddMinutes(2);
			Assert.IsFalse(now.Is().Not(1).Minutes().OlderThan(then));
			Assert.IsTrue(now.Is().Not(3).Minutes().OlderThan(then));
		}

		[TestMethod]
		public void IsNotSecOlder()
		{
			var now = DateTime.UtcNow;
			var then = now.AddSeconds(20);
			Assert.IsFalse(now.Is().Not(10).Seconds().OlderThan(then));
			Assert.IsTrue(now.Is().Not(30).Seconds().OlderThan(then));
		}

		[TestMethod]
		public void IsNotHoursOlder()
		{
			var now = DateTime.UtcNow;
			var then = now.AddHours(20);
			Assert.IsFalse(now.Is().Not(10).Hours().OlderThan(then));
			Assert.IsTrue(now.Is().Not(30).Hours().OlderThan(then));
		}
	}
}

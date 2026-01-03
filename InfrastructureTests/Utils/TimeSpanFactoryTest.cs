using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using static Infrastructure.GlobalFor;

namespace InfrastructureTests.Utils
{
	[TestClass]
	public class TimeSpanFactoryTest
	{
		[TestMethod]
		public void Ticks()
		{
			Assert.AreEqual(For(1), TimeSpan.FromTicks(1));
		}

		[TestMethod]
		public void Seconds()
		{
			Assert.AreEqual(For(1).Seconds(), TimeSpan.FromSeconds(1));
		}

		[TestMethod]
		public void Minutes()
		{
			Assert.AreEqual(For(1).Minutes(), TimeSpan.FromMinutes(1));
		}

		[TestMethod]
		public void Milliseconds()
		{
			Assert.AreEqual(For(1).Milliseconds(), TimeSpan.FromMilliseconds(1));
		}
	}
}

using Infrastructure;
using Infrastructure.Logger;
using Infrastructure.Logger.Tracers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InfrastructureTests.Logging
{
	[TestClass]
	public class MemoryTraceListenerTests
	{
		[TestInitialize]
		public void Initialize()
		{
			ServiceLocator.Reset();
			InMyTest.RememberAllEvents();
			InMyTest.UseInMemoryLogger();
		}

		[TestCleanup]
		public void Cleanup()
		{
			ServiceLocator.Reset();
		}

		[TestMethod]
		public void FiresEventOnTrace()
		{
			Log.Info(GetType(), "Hello");
			Assertions.AssertLogContains("Hello");
		}

	}
}

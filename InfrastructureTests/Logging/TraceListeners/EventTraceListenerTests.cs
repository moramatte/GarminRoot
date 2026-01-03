using Infrastructure;
using Infrastructure.Logger;
using Infrastructure.Logger.Tracers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InfrastructureTests.Logging
{
	[TestClass]
	public class EventTraceListenerTests
	{
		[TestInitialize]
		public void Initialize()
		{
			ServiceLocator.Reset();
			InMyTest.RememberAllEvents();
			var logger = InMyTest.UseInMemoryLogger();
			var eventTraceListener = new EventTraceListener(null);
			logger.AddListeners(eventTraceListener);
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
			Events.Assert<Logged>(x => x.Message.Contains("Hello"));
		}

	}
}

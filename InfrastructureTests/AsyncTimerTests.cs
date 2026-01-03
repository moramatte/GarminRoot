using Infrastructure;
using Infrastructure.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace InfrastructureTests
{
    [TestClass]
    public class AsyncTimerTests
    {
        AsyncScheduler _timer;

        [TestInitialize]
        public void Setup()
        {
            ServiceLocator.Reset();
            InMyTest.UseInMemoryLogger();
        }

        [TestMethod]
        public async Task AsyncScheduler_restarts_when_changing_duetime_and_interval()
        {
            var counter = 0;
            _timer = new AsyncScheduler(async (e) => counter++, 100, 100);
            _timer.Start();
            await Task.Delay(150);
            Assert.AreEqual(1, counter);
            await _timer.RestartAsync(200, 200);
            await Task.Delay(250);
            Assert.AreEqual(2, counter);
        }

		[TestMethod]
		public async Task AsyncScheduler_can_restart()
		{
			var counter = 0;
			_timer = new AsyncScheduler(async (e) => counter++, 1000, 1000);
			_timer.Start();
			await _timer.RestartAsync(200, 200);
			await Task.Delay(250);
			Assert.AreEqual(1, counter);
		}

	}
}

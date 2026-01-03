using FakeItEasy;
using Infrastructure;
using Infrastructure.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace InfrastructureTests
{
    [TestClass]
    public class ResettableDelayedExecutorTests
	{
        int _calls = 0;

        [TestInitialize]
        public void SetUp()
        {
            _calls = 0;
        }

        [TestMethod]
        public async Task Countdown_elapsed_triggers_callback()
        {
            var rde = new ResettableDelayedExecutor(TimeSpan.FromMilliseconds(300), Set);
            rde.SetCountdown();
            await Task.Delay(400);
            Assert.AreEqual(1, _calls);
        }

		[TestMethod]
		public async Task Countdown_resets_on_multible_calls_before_triggers_callback()
		{
			var rde = new ResettableDelayedExecutor(TimeSpan.FromMilliseconds(300), Set);
			rde.SetCountdown();
			await Task.Delay(200);
			rde.SetCountdown();
			await Task.Delay(200);
			Assert.AreEqual(0, _calls);
			await Task.Delay(200);
			Assert.AreEqual(1, _calls);
		}

		[TestMethod]
		public async Task Countdown_can_be_stopped()
		{
			var rde = new ResettableDelayedExecutor(TimeSpan.FromMilliseconds(300), Set);
			rde.SetCountdown();
			rde.StopCountdown();
			await Task.Delay(400);
			Assert.AreEqual(0, _calls);
		}

		private void Set()
        {
            _calls++;
        }
    }
}
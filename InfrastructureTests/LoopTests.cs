using System;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Infrastructure.GlobalFor;

namespace InfrastructureTests
{
	[TestClass]
	public class LoopTests
	{
        [TestMethod, Flaky, TestCategory(TestCategories.Flaky)]
        public async Task LoopsCanBeExpressed()
        {
            int counter = 0;
            await Loop.EverySecond(() => counter++, Cancel.After(TimeSpan.FromSeconds(3)));
            Assert.AreEqual(3, counter);
        }

        [TestMethod]
        public async Task ActionsAreDisclosedOnExceptions()
        {
            InMyTest.UseInMemoryLogger();
            
			await Loop.EverySecond(() =>
            {
                int nolla = 0;
				Console.WriteLine($"Division by noll {150 / nolla}");
            }, Cancel.After(TimeSpan.FromSeconds(3)));

            Assertions.AssertLogContains(nameof(ActionsAreDisclosedOnExceptions));
        }

        [TestMethod]
        public async Task ActionsAreCancelledGlobally()
        {
            ServiceLocator.Reset();

            int counter = 0;
            Loop.EverySecond(() =>
            {
                counter++;
                if (counter > 8)
                {
                    Assert.Fail("Should never get here");
                }
            });
            await Task.Delay(For(3).Seconds());
            ServiceLocator.Resolve<GlobalCancellationToken>().Token.Cancel();

            Assertions.Greater(1, counter);
            Assertions.Less(7, counter);
        }
	}
}

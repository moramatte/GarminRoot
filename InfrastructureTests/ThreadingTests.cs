using Infrastructure;
using Infrastructure.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Extensions;
using Infrastructure.Logger;
using InfrastructureTests.Logging;

namespace InfrastructureTests
{
    [TestClass]
    public class ThreadingTests
    {
        [TestInitialize]
        public void Setup()
        {
            ServiceLocator.Reset();
            InMyTest.UseInMemoryLogger();
        }

        const string _taskErrorMessage = "TaskErrror";
        [TestMethod]
        public async Task FireAndForget_logs_on_task_error()
        {
            Task.Run(ThrowAsync).FireAndForget();
            //Wait for the task to run
            //We cant wait for the task itself in the test as then we would catch the error ourselfs
            await Assertions.WaitForAssertion(() =>
            {
                var logger = ServiceLocator.Resolve<ILogger>() as InMemoryLogger;
                logger.AssertContains(_taskErrorMessage);
            }, 100);
        }

        private static async Task ThrowAsync() 
        {
            await Task.Delay(1);
            throw new Exception(_taskErrorMessage); 
        }

        [TestMethod]
        public async Task AsyncAutoResetEvent_can_be_cancelled()
        {
            var canceled = false;
            var resetEvent = new AsyncAutoResetEvent(false);
            var worker = Create.Worker(
            async (a, e) =>
            {
                await resetEvent.WaitAsync(e);
            }, 
            (e) =>
            {
                canceled = e.Canceled;
            });
            worker.RunWorkerAsync(null).FireAndForget();
            await Task.Delay(50);
            worker.Cancel();
            await Task.Delay(50);
            Assert.IsTrue(canceled);
        }

        [TestMethod]
        public async Task AsyncAutoResetEvent_can_timeout()
        {
            var resetEvent = new AsyncAutoResetEvent(false);
            var resetTask = resetEvent.WaitAsync(10, CancellationToken.None);
            await Task.WhenAny(resetTask, Task.Delay(100));
            Assert.IsTrue(resetTask.IsCompleted);
        }

        [TestMethod]
        public async Task TaskHandler_returns_error_if_workerfunc_throws()
        {
            var hasThrown = false;
            var worker = Create.Worker(
            async (a, e) =>
            {
                throw new Exception("Worker throws");
            }, 
            (e) =>
            {
                hasThrown = e.Error != null;
            });
            await worker.RunWorkerAsync(null);
            Assert.IsTrue(hasThrown);
        }

        [TestMethod]
        public async Task TaskTracker_closes_workers_on_dispose()
        {
            var provider = ServiceLocator.Resolve<IAsyncWorkerFactory>();

            var worker = Create.Worker(async (a, e) =>
            {
                while (!e.IsCancellationRequested)
                {
                    await Task.Delay(10, e);
                }
            });

            //Wait for the workers task to have started
            worker.RunWorkerAsync(null).FireAndForget();
            await Task.Delay(500);

            //Disposing should then stop the worker
            provider.Dispose();

            Assert.IsFalse(worker.IsBusy);
        }

        [TestMethod]
        public async Task TaskTracker_doesnt_throw_when_timer_stops_on_dispose()
        {
            var provider = ServiceLocator.Resolve<IAsyncWorkerFactory>();

            const int interval = 200;
            var value = 0;
            var timer = new AsyncScheduler((e) =>
            {
                value++;
                return Task.CompletedTask;
            }, interval, interval);
            timer.Start();

            await Task.Delay(interval + (int)(interval * 0.5f));
            provider.Dispose();
            await Task.Delay(interval);
            
            //As long as the timer only elapsed once we can assume the timer was stopped
            Assert.AreEqual(1, value);
        }

        [TestMethod]
        public async Task TaskTracker_throws_when_timer_doesnt_stop_on_dispose()
        {
            var provider = ServiceLocator.Resolve<IAsyncWorkerFactory>();

            var timer = new AsyncScheduler(async (e) =>
            {
                await Task.Delay(10000, CancellationToken.None);
            }, 0, 0);
            timer.Start();

            await Task.Delay(5000);

            try
            {
                provider.Dispose();
            }
            catch (TimeoutException)
            {
                return;
            }

            Assert.Fail("Did not throw as expected");
        }

        [TestMethod]
        public async Task TaskTracker_clears_completed_workers()
        {
            var provider = ServiceLocator.Resolve<IAsyncWorkerFactory>();

            for (int i = 0; i < 10; ++i)
            {
                TaskHandler.Run(NoWork);
            }
            Assert.AreEqual(10, provider.AliveWorkerOwners.Count());

            await Task.Delay(100);
            GC.Collect();

            TaskHandler.Run(NoWork);
            Assert.AreEqual(1, provider.AliveWorkerOwners.Count());
        }

        private async Task NoWork(CancellationToken token) { }
    }
}

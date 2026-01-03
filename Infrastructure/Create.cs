using Infrastructure.Threading;
using System.Runtime.CompilerServices;

namespace Infrastructure
{
    public static class Create
    {
        public static IAsyncWorker Worker(DoWorkEventHandler handler, WhenCompleteHandler completeHandler = null, [CallerFilePath] string caller = null)
        {
            return ServiceLocator.Resolve<IAsyncWorkerFactory>().CreateWorker(handler, completeHandler, caller);
        }

        public static IAsyncScheduler Scheduler(TimeElapsedHandler handler, int dueTimeMs, int intervalTimeMs, [CallerFilePath] string caller = null)
        {
            return ServiceLocator.Resolve<IAsyncSchedulerFactory>().CreateScheduler(handler, dueTimeMs, intervalTimeMs, caller);
        }
    }
}

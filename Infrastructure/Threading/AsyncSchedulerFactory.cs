using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Infrastructure.Threading;

internal class AsyncSchedulerFactory : IAsyncSchedulerFactory
{
    public IAsyncScheduler CreateScheduler(TimeElapsedHandler handler, int dueTimeMs, int intervalTimeMs, [CallerFilePath] string caller = null)
    {
        return new AsyncScheduler(handler, dueTimeMs, intervalTimeMs, caller);
    }
}

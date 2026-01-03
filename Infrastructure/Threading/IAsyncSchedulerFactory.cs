using System.Runtime.CompilerServices;

namespace Infrastructure.Threading;

public interface IAsyncSchedulerFactory
{
    public IAsyncScheduler CreateScheduler(TimeElapsedHandler handler, int dueTimeMs, int intervalTimeMs, [CallerFilePath] string caller = null);
}

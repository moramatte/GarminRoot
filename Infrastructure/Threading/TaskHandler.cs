using System.Runtime.CompilerServices;

namespace Infrastructure.Threading;

public static class TaskHandler
{
    public static IAsyncWorker Run(DoOneShotWorkEventHandler doWorkHandler, [CallerFilePath] string caller = null)
    {
        var tmp = Create.Worker((a, c) => doWorkHandler(c), null, caller);
        tmp.RunWorkerAsync();
        return tmp;
    }
}

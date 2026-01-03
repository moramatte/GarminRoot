using System;
using System.Threading.Tasks;

namespace Infrastructure.Threading;

public interface IAsyncWorker : IDisposable
{
    Task InternalTask { get; }
    bool IsBusy { get; }
    bool IsCanceled { get; }

    void Cancel();
    Task RunWorkerAsync(object args = null);
    void Wait();
}
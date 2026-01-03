using System.Collections.Generic;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Infrastructure.Logger;
using System.Linq;

namespace Infrastructure.Threading;

internal class AsyncWorkerFactory : IAsyncWorkerFactory
{
    public IAsyncWorker CreateWorker(DoWorkEventHandler doWorkHandler, WhenCompleteHandler whenCompleteHandler = null, [CallerFilePath] string caller = null)
    {
        var newWorker = new AsyncWorker(doWorkHandler, whenCompleteHandler, caller);
        Register(newWorker, caller);
        return newWorker;
    }

    private const int _cleanupTimeout = 1500; //1.5 seconds

    private object _lock = new();
    private List<TaskWorkerData> _workers = new();
    private struct TaskWorkerData
    {
        public bool IsGarbageCollected => !worker.TryGetTarget(out _);
        public WeakReference<IAsyncWorker> worker;
        public string caller;
        public TaskWorkerData(IAsyncWorker taskWorker, string workerCaller)
        {
            worker = new(taskWorker);
            caller = workerCaller;
        }
    }

    public IEnumerable<string> AliveWorkerOwners => _workers.Select(x => x.caller);

    private void Register(IAsyncWorker worker, [CallerFilePath] string caller = null)
    {
        lock (_lock)
        {
            _workers.Add(new(worker, caller));
            _workers.RemoveAll(x => x.IsGarbageCollected);
        }
    }

    public void Dispose()
    {
        List<TaskWorkerData> workerList;
        lock (_lock)
        {
            workerList = new(_workers);
            _workers.Clear();
        }

        var lingeringWorkers = 0;
        foreach (var workerData in workerList)
        {
            if (workerData.worker.TryGetTarget(out var worker))
            {
                var wasCanceled = worker.IsCanceled;
                if (!wasCanceled)
                {
                    worker.Dispose();
                    worker.Cancel();
                }

                var task = worker.InternalTask;
                if (task != null)
                {
                    var winner = Task.WaitAny(task, Task.Delay(_cleanupTimeout));
                    if (winner == 1)
                    {
                        ServiceLocator.Resolve<ILogger>().Error($"A worker from {workerData.caller} did not finish within set cleanup timeout", nameof(AsyncWorkerFactory));
                        lingeringWorkers++;
                    }
                    if (!wasCanceled)
                    {
                        ServiceLocator.Resolve<ILogger>().Warning($"Cleaned up taskworker from {workerData.caller}", nameof(AsyncWorkerFactory));
                    }
                }
            }
        }

        if (lingeringWorkers > 0)
        {
            throw new TimeoutException($"{lingeringWorkers} worker(s) did not finish within set cleanup timeout");
        }
    }
}

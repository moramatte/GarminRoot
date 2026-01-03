using Infrastructure.Logger;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Threading
{
	public struct AsyncWorkerResult
    {
        public Exception Error;
        public bool Canceled;
    }

    public delegate Task DoWorkEventHandler(object args, CancellationToken token);
    public delegate Task DoOneShotWorkEventHandler(CancellationToken token);
    public delegate void WhenCompleteHandler(AsyncWorkerResult result);

    internal class AsyncWorker : IAsyncWorker
    {
        private WhenCompleteHandler _completedHandler;
        private CancellationTokenSource _tokenSource;
        private DoWorkEventHandler _taskHandler;
        private object _startLock;
        private string _caller;
        private bool _isBusy;
        private bool _disposed;
        private Task _task;

        public Task InternalTask => _task;
        public bool IsBusy => _isBusy;

        public AsyncWorker(DoWorkEventHandler doWorkHandler, WhenCompleteHandler whenCompleteHandler = null, [CallerFilePath] string caller = null)
        {
            _caller = caller ?? string.Empty;
            _taskHandler = doWorkHandler;
            _completedHandler = whenCompleteHandler;
            _startLock = new object();
        }

        /// <summary>
        /// Starts the assigned task
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Task RunWorkerAsync(object args = null)
        {
            lock (_startLock)
            {
                if (_disposed)
                    throw new ObjectDisposedException("Worker has been disposed");
                if (_isBusy) 
                    return _task;
                _isBusy = true;
            }

            _tokenSource = new CancellationTokenSource();
            _task = Task.Run(() => DoWork(args), _tokenSource.Token);
            return _task;
        }

        private async Task DoWork(object args)
        {
            var result = new AsyncWorkerResult();
            try
            {
                await _taskHandler?.Invoke(args, _tokenSource.Token);
            }
            catch (Exception ex)
            {
                result.Error = ex;
            }
			_isBusy = false;
            result.Canceled = _tokenSource.IsCancellationRequested;
            _completedHandler?.Invoke(result);
        }

        public bool IsCanceled
        {
            get
            {
                return _tokenSource?.IsCancellationRequested != false;
            }
        }

        /// <summary>
        /// Requests the worker to stop
        /// </summary>
        public void Cancel()
        {
            try
            {
                _tokenSource?.Cancel();
            }
            catch (ObjectDisposedException e)
            {
                ServiceLocator.Resolve<ILogger>().Error($"Task already disposed. caller:{_caller}", nameof(IAsyncWorker), e);
            }
            catch (AggregateException e)
            {
                ServiceLocator.Resolve<ILogger>().Error($"Error during task cancel. caller:{_caller}", nameof(IAsyncWorker), e);
            }

			_isBusy = false;
		}

        /// <summary>
        /// Waits for the running task to finish<br />
        /// Not supported on browser platform
        /// </summary>
        public void Wait()
        {
            _task?.Wait();
        }

		public void Dispose()
		{
            _disposed = true;
		}
	}
}

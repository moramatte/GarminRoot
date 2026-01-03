using Infrastructure.Extensions;
using Infrastructure.Logger;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Threading
{
    public delegate Task TimeElapsedHandler(CancellationToken token);
    internal class AsyncScheduler : IAsyncScheduler
    {
        private readonly IAsyncWorker _worker;
        private readonly string _caller;

        /// <summary>
        /// Creates a new AsyncScheduler
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="dueTimeMs"></param>
        /// <param name="intervalTimeMs"></param>
        /// <param name="caller"></param>
        /// <param name="callerMemberName"></param>
        /// <param name="callerLineNumber"></param>
        public AsyncScheduler(TimeElapsedHandler handler, int dueTimeMs, int intervalTimeMs, [CallerFilePath] string caller = null)
        {
            Interval = intervalTimeMs;
            _dueTime = dueTimeMs;
            _elapsed = handler;
            _caller = caller ?? string.Empty;
            _worker = Create.Worker(TimerWork, TimerStopped, caller);
        }

		private void TimerStopped(AsyncWorkerResult result)
		{
            if (!result.Canceled && result.Error != null)
                Log.Error(GetType(), $"Timer from {_caller} was stopped", result.Error);
		}

        private async Task TimerWork(object args, CancellationToken token)
        {
            if (_dueTime > 0 && _dueTime != Timeout.Infinite)
                await Task.Delay(_dueTime, token);
            else if (Interval > 0 && Interval != Timeout.Infinite)
                await Task.Delay(Interval, token);

            while (!token.IsCancellationRequested)
            {
                if (Enabled)
                {
                    await _elapsed?.Invoke(token);
                }

                if (Interval == Timeout.Infinite || Interval < 0)
                    break;

                await Task.Delay(Interval, token);
            }
        }

        /// <summary>
        /// true if the Timer should raise the invoke handler; otherwise, false.
        /// </summary>
        public bool Enabled { get; private set; }

        /// <summary>
        /// The time, in milliseconds, between handler invoked. <br />
        /// The value must be greater than zero, and less than or equal to Int32.MaxValue else the timer is stopped.
        /// </summary>
        public int Interval { get; set; }

        /// <summary>
        /// The time, in milliseconds, before the first/intial invoke of handler. <br />
        /// The value must be greater than zero, and less than or equal to Int32.MaxValue else the interval is used as dueTime.
        /// </summary>
        private int _dueTime;

        /// <summary>
        /// Occurs when the interval elapses.
        /// </summary>
        private TimeElapsedHandler _elapsed;

        private bool HasTime => !(_dueTime == Timeout.Infinite && Interval == Timeout.Infinite);

        /// <summary>
        /// Starts invoking the handler by setting Enabled to true.</br>
        /// If this is the first time start is called, work starts after dueTime + periodTime
        /// </summary>
        public void Start()
        {
            Enabled = true;
            if (!_worker.IsBusy && HasTime)
            {
                _worker.RunWorkerAsync().FireAndForget();
            }
        }

        /// <summary>
        /// Restarts the scheduler with set due and interval time
        /// </summary>
        public void Restart() => Restart(_dueTime, Interval);

        /// <summary>
        /// Restarts the scheduler with new due and interval time
        /// </summary>
        /// <param name="seconds">new due and interval time</param>
        public void Restart(int seconds) => Restart(seconds * 1000, seconds * 1000);

        bool _pendingRestart;

        /// <summary>
        /// Restarts the scheduler with new due and interval time
        /// </summary>
        /// <param name="dueTimeMs"></param>
        /// <param name="periodMs"></param>
        public void Restart(int dueTimeMs, int periodMs) => RestartAsync(dueTimeMs, periodMs).FireAndForget();

        public Task RestartAsync(int dueTimeMs, int periodMs)
        {
            Stop();
            Interval = periodMs;
            _dueTime = dueTimeMs;
            if (_pendingRestart)
                return Task.CompletedTask;
            _pendingRestart = true;
            //Restart the scheduler worker on a separate task to prevent possible deadlocks
            //that could occur if restart was called within worker handler
            return Task.Run(async () =>
            {
                try
                {
                    if (_worker.InternalTask != null)
                    {
                        var success = await AwaitWithTimeout(_worker.InternalTask, TimeSpan.FromMinutes(1));
                        if (!success)
                        {
                            Log.Warning(GetType(), $"Timeout when attempting to restart scheduler {_caller}");
                        }
                    }

                    Start();
                }
                finally
                {
                    _pendingRestart = false;
                }
            });
        }

        private async Task<bool> AwaitWithTimeout(Task toAwait, TimeSpan timeout)
        {
            var completed = toAwait;
            try
            {
                completed = await Task.WhenAny(toAwait, Task.Delay(timeout));
            }
            catch (Exception e) 
            {
                Log.Warning(GetType(), $"{_caller} Exception in current task during restart: {e}");
                throw;
            }
            return completed == toAwait;
        }

        /// <summary>
        /// Stops raising the Elapsed event by setting Enabled to false.
        /// </summary>
        public void Pause()
        {
            Enabled = false;
        }

        /// <summary>
        /// Stops the timer by stopping the background task
        /// </summary>
        public void Stop()
        {
            Pause();
            _worker.Cancel();
        }

        /// <summary>
        /// Stops the timer
        /// </summary>
		public void Dispose()
        {
            Stop();
        }
    }
}

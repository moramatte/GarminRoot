using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace Infrastructure.Threading
{
    public class AsyncAutoResetEvent : IDisposable
    {
        readonly LinkedList<TaskCompletionSource<bool>> waiters = new();
        readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        bool isSignaled;

        public AsyncAutoResetEvent(bool signaled)
        {
            isSignaled = signaled;
        }

        public void Dispose()
        {
            _semaphoreSlim.Dispose();
        }

        public Task<bool> WaitAsync(CancellationToken cancellationToken)
        {
            return WaitAsync(Timeout.InfiniteTimeSpan, cancellationToken);
        }

        public Task<bool> WaitAsync(int ms, CancellationToken token)
        {
            return WaitAsync(TimeSpan.FromMilliseconds(ms), token);
        }

        public async Task<bool> WaitAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            TaskCompletionSource<bool> tcs;

            await _semaphoreSlim.WaitAsync(cancellationToken);
			if (isSignaled)
            {
                isSignaled = false;
				_semaphoreSlim.Release();
				return true;
            }
            else if (timeout == TimeSpan.Zero)
            {
				_semaphoreSlim.Release();
				return isSignaled;
            }
            
            tcs = new TaskCompletionSource<bool>();
            waiters.AddLast(tcs);
            _semaphoreSlim.Release();

            var winner = await Task.WhenAny(tcs.Task, Task.Delay(timeout, cancellationToken));
            if (winner == tcs.Task)
            {
                // The task was signaled.
                return true;
            }
            else
            {
				// We timed-out; remove our reference to the task.
				// This is an O(n) operation since waiters is a LinkedList<T>.
				await _semaphoreSlim.WaitAsync(cancellationToken);
                var removed = waiters.Remove(tcs);
                _semaphoreSlim.Release();
                //In the odd case we have already been removed from waiters
                //we assume that we were signaled while waiting for lock for waiters
                //therefore we return true/signaled if we failed to remove ourselfs
                return !removed;
            }
        }

        public void Set()
        {
            _semaphoreSlim.Wait();
            if (waiters.Count > 0)
            {
                // Signal the first task in the waiters list. This must be done on a new
                // thread to avoid stack-dives and situations where we try to complete the
                // same result multiple times.
                var tcs = waiters.First.Value;
                Task.Run(() => tcs.SetResult(true));
                waiters.RemoveFirst();
            }
            else if (!isSignaled)
            {
                // No tasks are pending
                isSignaled = true;
            }
            _semaphoreSlim.Release();
        }

        public void Reset()
        {
            isSignaled = false;
        }

        public override string ToString()
        {
            return $"Signaled: {isSignaled}, Waiters: {waiters.Count}";
        }
    }
}

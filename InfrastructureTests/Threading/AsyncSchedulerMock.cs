using Infrastructure.Threading;
using System.Threading;
using System.Threading.Tasks;

namespace InfrastructureTests.Threading
{
    internal class AsyncSchedulerMock : IAsyncSchedulerMock
    {
        public bool Enabled { get; set; }
        public int Interval { get; set; }

        TimeElapsedHandler _handler;

        public AsyncSchedulerMock(TimeElapsedHandler timeElapsedHandler, int interval) 
        {
            _handler = timeElapsedHandler;
            Interval = interval;
        }

        public void Dispose() { }
        public void Pause() { Enabled = false; }
        public void Restart() { Enabled = true; }
        public void Restart(int seconds) { Enabled = true; }
        public void Restart(int dueTimeMs, int periodMs)
        {
            Enabled = true;
        }

        public void Start() { Enabled = true; }
        public void Stop() { Enabled = false; }

        public TimeElapsedHandler GetHandler() => _handler;

        public Task Tick(CancellationToken token)
        {
            return _handler.Invoke(token);
        }
    }
}

using System;
using System.Threading.Tasks;

namespace Infrastructure.Threading;

public interface IAsyncScheduler : IDisposable
{
    bool Enabled { get; }
    int Interval { get; set; }

    void Pause();
    void Restart();
    void Restart(int seconds);
    void Restart(int dueTimeMs, int periodMs);
    void Start();
    void Stop();
}
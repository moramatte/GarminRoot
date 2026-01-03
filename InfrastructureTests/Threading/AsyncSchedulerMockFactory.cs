using Infrastructure.Threading;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Infrastructure.Extensions;

namespace InfrastructureTests.Threading;

public class AsyncSchedulerMockFactory : IAsyncSchedulerFactory
{
    List<AsyncSchedulerMock> _handlers = new();
    public IAsyncScheduler CreateScheduler(TimeElapsedHandler handler, int dueTimeMs, int intervalTimeMs, [CallerFilePath] string caller = null)
    {
        var mock = new AsyncSchedulerMock(handler, intervalTimeMs);
        _handlers.Add(mock);
        return mock;
    }

    public IAsyncSchedulerMock FindScheduler(string invokedMethodName, object target)
    {
        if (invokedMethodName.HasContent())
            return _handlers.Find(x => x.GetHandler().Target == target && x.GetHandler().Method.Name.Equals(invokedMethodName, StringComparison.Ordinal));
        return _handlers.Find(x => x.GetHandler().Target == target);
    }
}

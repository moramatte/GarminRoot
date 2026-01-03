using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Infrastructure.Threading;

public interface IAsyncWorkerFactory : IDisposable
{
    IAsyncWorker CreateWorker(DoWorkEventHandler doWorkHandler, WhenCompleteHandler whenCompleteHandler = null, [CallerFilePath] string caller = null);
    IEnumerable<string> AliveWorkerOwners { get; }
}

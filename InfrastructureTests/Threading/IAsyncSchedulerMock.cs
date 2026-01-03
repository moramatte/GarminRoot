using Infrastructure.Threading;
using System.Threading;
using System.Threading.Tasks;

namespace InfrastructureTests.Threading;

public interface IAsyncSchedulerMock : IAsyncScheduler
{
    /// <summary>
    /// Invokes the handler<br />
    /// Should really only be used in tests
    /// </summary>
    Task Tick(CancellationToken token);
}
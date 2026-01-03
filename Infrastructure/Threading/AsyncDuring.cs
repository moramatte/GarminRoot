using System;
using System.Threading.Tasks;

namespace InfrastructureTests.Threading
{
	public struct AsyncDuring(Task task) : IAsyncDisposable
	{
		Task _task = task;

		public async ValueTask DisposeAsync()
		{
			await _task;
		}
	}
}

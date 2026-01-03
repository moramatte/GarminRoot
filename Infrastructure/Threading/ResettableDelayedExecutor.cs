using System;
using System.Threading.Tasks;
using System.Threading;

namespace Infrastructure.Threading
{
	/// <summary>
	/// Executes a given action after a given delay
	/// </summary>
	public class ResettableDelayedExecutor
	{
		private readonly TimeSpan _delay;
		private readonly Action _action;
		private CancellationTokenSource _cancellationTokenSource;
		private readonly object _lock = new object();

		public ResettableDelayedExecutor(TimeSpan delay, Action action)
		{
			_delay = delay;
			_action = action;
		}

		/// <summary>
		/// Resets/Start the countdown
		/// </summary>
		public void SetCountdown()
		{
			lock (_lock)
			{
				_cancellationTokenSource?.Cancel();
				_cancellationTokenSource = new CancellationTokenSource();
				var token = _cancellationTokenSource.Token;

				Task.Delay(_delay, token).ContinueWith(t =>
				{
					if (!t.IsCanceled)
					{
						_action();
					}
				});
			}
		}

		/// <summary>
		/// Stops/Clears the countdown
		/// </summary>
		public void StopCountdown()
		{
			lock (_lock)
			{
				try
				{
					_cancellationTokenSource?.Cancel();
				} catch (Exception) { }
				_cancellationTokenSource = null;
			}
		}
	}
}

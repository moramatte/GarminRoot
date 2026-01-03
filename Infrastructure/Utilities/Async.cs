using Infrastructure.EventAggregator;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Utilities
{
	public class Async
    {
		/// <summary>
		/// Await for given lambda to evaluate to true
		/// </summary>
		/// <param name="condition">condition to evaluate</param>
		/// <param name="timeout">[milliseconds] for how long should we try to evaluate</param>
		/// <param name="checkInterval">[milliseconds] how often condition should be evaluated</param>
		/// <returns>true when condition evaluated to true, false when timeout</returns>
		public static async Task<bool> Condition(Func<bool> condition, int timeout = 10000, int checkInterval = 100)
        {
            while (!condition() && timeout > 0)
            {
                await Task.Delay(checkInterval);
                timeout -= checkInterval;
            }
            return timeout > 0;
        }

		public static async Task While(Func<bool> condition, int checkInterval = 100)
		{
			while (condition())
			{
				await Task.Delay(checkInterval);
			}
		}

        /// <summary>
        /// Task.Delay with guard for cancelled task exception
        /// </summary>
        /// <param name="millisecondsDelay"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task SafeDelay(int millisecondsDelay, CancellationToken token)
        {
            try
            {
                await Task.Delay(millisecondsDelay, token);
            }
            catch (TaskCanceledException) { }
        }

		/// <summary>
        /// Clock resolution is rumored to be about 15ms. <br />
        /// This delay waits for 30ms in addition to make sure we do not return from the wait early <br />
		/// See https://github.com/dotnet/runtime/issues/100455
		/// </summary>
		/// <param name="timeSpan"></param>
		/// <returns></returns>
		public static async Task DelayAtLeast(TimeSpan timeSpan)
        {
            await Task.Delay(timeSpan.Add(TimeSpan.FromMilliseconds(30)));
        }

        /// <summary>
        /// Task.Delay with guard for cancelled task exception
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task SafeDelay(TimeSpan timeSpan, CancellationToken token)
        {
            await SafeDelay((int)timeSpan.TotalMilliseconds, token);
        }
    }
}

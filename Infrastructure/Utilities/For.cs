using System;
using System.Threading.Tasks;

namespace Infrastructure.Utilities
{
    public static class For
    {
        /// <summary>
        /// Throws a TimeoutException if the predicate does not return true within the specified timeout period.
        /// </summary>
        public static async Task _(Func<bool> predicate, int timeoutMs)
        {
            var startTime = DateTime.UtcNow;
            while (!predicate())
            {
                await Task.Delay(150);

                var elapsed = DateTime.UtcNow - startTime;
                if (elapsed.TotalMilliseconds > timeoutMs)
                {
                    throw new TimeoutException("The operation has timed out.");
                }
            }
        }

        public static async Task ThreeSeconds()
        {
            await Task.Delay(3000);
        }
    }
}

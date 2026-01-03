using System;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Utilities
{
    public static class After
    {
        public static async Task ThirtySeconds(Action action, CancellationToken token)
        {
            await XSeconds(30, action, token);
        }

        public static async Task TenSeconds(Action action, CancellationToken token)
        {
            await XSeconds(10, action, token);
        }

        public static async Task ThirtySeconds(Action action)
        {
            var globalToken = ServiceLocator.Resolve<GlobalCancellationToken>().Token.Token;
            await XSeconds(30, action, globalToken);
        }

        public static async Task TenSeconds(Action action)
        {
            var globalToken = ServiceLocator.Resolve<GlobalCancellationToken>().Token.Token;
            await XSeconds(10, action, globalToken);
        }

        private static async Task XSeconds(int seconds, Action action, CancellationToken token)
        {
            var wasCancelled = await Cancellation(token, seconds * 1000);
            if (wasCancelled)
            {
                return;
            }

            action();
        }

        private static async Task<bool> Cancellation(CancellationToken token, int milliseconds)
        {
            var startTime = DateTime.Now;

            while ((DateTime.Now - startTime).TotalMilliseconds < milliseconds)
            {
                if (token.IsCancellationRequested)
                    return true;
                await Task.Delay(50);
            }

            return false;
        }
    }
}

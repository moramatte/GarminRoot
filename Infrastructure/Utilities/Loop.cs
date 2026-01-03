using System;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Logger;

namespace Infrastructure.Utilities
{
	public static class Loop
	{
        public static async Task EverySecond(Action action)
        {
            var globalToken = ServiceLocator.Resolve<GlobalCancellationToken>().Token.Token;
            await EveryX(action, globalToken, 1);
        }

        public static async Task EveryMinute(Action action)
        {
            var globalToken = ServiceLocator.Resolve<GlobalCancellationToken>().Token.Token;
            await EveryX(action, globalToken, 60);
        }

        public static async Task EveryTwoMinutes(Action action, CancellationToken token)
        {
            await EveryX(action, token, 60 * 2);
        }

        public static async Task EverySecond(Action action, CancellationToken cancellationToken)
        {
            await EveryX(action, cancellationToken, 1);
        }

        private static async Task EveryX(Action action, CancellationToken cancellationToken, int seconds)
        {
            await Task.Run(() =>
            {
                while (cancellationToken.Active())
                {
                    Task.Delay(TimeSpan.FromSeconds(seconds)).Wait();

                    try
                    {
                        action();
                    }
                    catch (Exception e)
                    {
                        var message = $"Failure executing {action.Method.Name}";
                        Log.Error(typeof(Loop), message, e);
                    }
                }
            });
		}
	}
}

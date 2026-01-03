using System;
using System.Threading;

namespace Infrastructure
{
	public static class Cancel
	{
		public static CancellationToken After(int ms)
		{
			var cancelTokenSource = new CancellationTokenSource(ms);
			cancelTokenSource.Token.Register(cancelTokenSource.Dispose);
			return cancelTokenSource.Token;
		}

		public static CancellationToken After(int ms, CancellationToken linked)
		{
			var linkd = CancellationTokenSource.CreateLinkedTokenSource(After(ms), linked);
			linkd.Token.Register(linkd.Dispose);
			return linkd.Token;
		}

		public static CancellationToken After(TimeSpan time)
		{
			return After((int)time.TotalMilliseconds);
		}

        public static CancellationToken AfterAWhile()
        {
            return After(TimeSpan.FromMinutes(Chance.Within(1, 10)));
        }
	}
}

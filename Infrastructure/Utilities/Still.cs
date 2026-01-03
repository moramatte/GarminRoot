using System.Threading;

namespace Infrastructure.Utilities
{
	public static class Still
	{
        public static bool Active(this CancellationToken token)
        {
            return !token.IsCancellationRequested;
        }
	}
}

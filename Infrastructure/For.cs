using System;
using Infrastructure.Utilities;

namespace Infrastructure
{
	public static class GlobalFor
	{
        public static TimeSpan ForAWhile()
        {
            if (Execution.IsRunningFromTest())
            {
                return TimeSpan.FromSeconds(Chance.Within(5, 100));
			}
            return TimeSpan.FromMinutes(Chance.Within(1, 6));
        }

        public static TimeSpanFactory For(double time)
        {
            return new TimeSpanFactory(time);
        }
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Utilities
{
	public enum TimeUnit
	{
		Ticks, Seconds, Milliseconds, Minutes
	}

	public ref struct TimeSpanFactory(double time)
	{
		readonly double _time = time;
		TimeUnit _unit = TimeUnit.Ticks;

		public TimeSpanFactory Seconds()
		{
			_unit = TimeUnit.Seconds;
			return this;
		}

		public TimeSpanFactory Milliseconds()
		{
			_unit = TimeUnit.Milliseconds;
			return this;
		}

		public TimeSpanFactory Minutes()
		{
			_unit = TimeUnit.Minutes;
			return this;
		}

		public static implicit operator TimeSpan(TimeSpanFactory timeSpanFactory)
		{
			switch (timeSpanFactory._unit)
			{
				case TimeUnit.Minutes:
					return TimeSpan.FromMinutes(timeSpanFactory._time);
				case TimeUnit.Seconds:
					return TimeSpan.FromSeconds(timeSpanFactory._time);
				case TimeUnit.Milliseconds:
					return TimeSpan.FromMilliseconds(timeSpanFactory._time);
				case TimeUnit.Ticks:
				default:
					return TimeSpan.FromTicks((long)timeSpanFactory._time);
			}
		}
	}

    public static class Yesterday
    {
        public static DateTime Evening()
        {
            var now = DateTime.UtcNow;
            var yesterday = now - TimeSpan.FromDays(1);
            return new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 18, 0, 0);
        }
    }
}

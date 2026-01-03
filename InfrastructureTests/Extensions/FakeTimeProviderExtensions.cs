using FakeItEasy;
using Infrastructure;
using System;

namespace InfrastructureTests.Extensions
{
	public static class FakeTimeProviderExtensions
	{
		public static void SetTime(this ITimeProvider timeProvider, int year, int month, int day)
		{
			SetTime(timeProvider, new DateTime(year, month, day));
		}

		public static void SetTime(this ITimeProvider timeProvider, int year, int month, int day, int hour, int minute, int second)
		{
			SetTime(timeProvider, new DateTime(year, month, day, hour, minute, second));
		}

		public static void SetTime(this ITimeProvider timeProvider, DateTime time)
		{
			A.CallTo(() => timeProvider.UtcNow).Returns(time);
			A.CallTo(() => timeProvider.Now).Returns(time);
			A.CallTo(() => timeProvider.Today).Returns(time.Date);
		}

		public static void FastForwardAMinute(this ITimeProvider timeProvider)
		{
			var now = timeProvider.UtcNow;
			var newTime = now + TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(1);

			SetTime(timeProvider, newTime);
		}

		public static void IncrementBySeconds(this ITimeProvider timeProvider, double seconds = 1)
		{
			var now = timeProvider.UtcNow;
			var newTime = now + TimeSpan.FromSeconds(seconds);

			SetTime(timeProvider, newTime);
		}
	}

}

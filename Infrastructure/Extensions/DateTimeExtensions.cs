using Infrastructure.Utilities;
using System;
using System.Diagnostics;

namespace Infrastructure.Extensions
{
	public static class DateTimeExtensions
	{
        [DebuggerStepThrough]
        public static DateTimeLengthComparer Is(this DateTime first, double amount = double.NaN)
        {
            return new DateTimeLengthComparer(first, amount);
        }

        public static string ToHtmlDateTimeLocal(this DateTime dateTime)
        {
            return $"{dateTime:yyyy-MM-ddTHH:mm:ss}";
        }
	}
}

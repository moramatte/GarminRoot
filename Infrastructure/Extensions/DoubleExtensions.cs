using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Infrastructure.Extensions
{
	public static class DoubleExtensions
	{
		public static bool IsOk(this double v)
		{
			return !double.IsNaN(v);
		}

        public static bool IsNaN(this double v)
        {
            return double.IsNaN(v);
        }

		public static double Round(this double a)
		{
			return Math.Round(a, MidpointRounding.ToEven);
		}
	}
}

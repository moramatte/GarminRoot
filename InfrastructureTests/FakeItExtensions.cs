using FakeItEasy;
using FakeItEasy.Configuration;
using System;
using System.Linq;

namespace InfrastructureTests
{
	public static class FakeItExtensions
	{
		public static TInterface Method<TInterface>(this IWhereConfiguration<TInterface> anyCall, string methodName, params object[] args)
		{
			if (args.Length == 0)
				return anyCall.Where(call => call.Method.Name.Equals(methodName, StringComparison.Ordinal));
			else
				return anyCall.Where(call => call.Method.Name.Equals(methodName, StringComparison.Ordinal) && call.Arguments.SequenceEqual(args));
		}

		public static TInterface Setter<TInterface>(this IWhereConfiguration<TInterface> anyCall, string propertyName)
		{
			return anyCall.Where(call => call.Method.Name.Equals($"set_{propertyName}", StringComparison.Ordinal));
		}

		public static object[] ArgsFromLastCallTo<T>(this Fake<T> fake, string methodName) where T : class
		{
			return fake.RecordedCalls.Where(x => x.Method.Name == methodName).Last().Arguments.ToArray();
		}
	}
}

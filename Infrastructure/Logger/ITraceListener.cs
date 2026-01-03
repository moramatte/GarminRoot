using Awos7.WindowsService.LogConfig;
using Infrastructure.Logger.Enterprise;
using Infrastructure.Logger.Formatters;
using System;
using System.Diagnostics;

namespace Infrastructure.Logger
{
	public interface ITraceListener : IDisposable
	{
		string Name { get; }
		TraceFilter Filter { get; }
		ILogFormatter Formatter { get; }

		void Write(TraceEventCache eventCache, string source, TraceEventType eventType, int id, LogEntry data);
		void Flush();

		TraceListenerConfig ToConfig();
	}

	public static class ITraceListenerExtensions
	{
		public static TraceListenerConfig CommonConfig(this ITraceListener traceListener)
		{
			return new TraceListenerConfig()
			{
				Name = traceListener.Name,
				Filter = (traceListener.Filter as LogLevelFilter)?.EventType ?? TraceEventType.Verbose,
				Formatter = ServiceLocator.Resolve<IFormatterRepository>().Serialize(traceListener.Formatter),
			};
		}
	}
}

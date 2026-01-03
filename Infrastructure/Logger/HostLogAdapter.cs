using Microsoft.Extensions.Logging;
using System;

namespace Infrastructure.Logger
{
	public class HostLogAdapterProvider : ILoggerProvider
	{
		string _caller;
		public HostLogAdapterProvider(string caller)
		{
			_caller = caller;
		}

		public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
		{
			return new HostLogAdapter(_caller);
		}

		public void Dispose()
		{

		}
	}

	public class HostLogAdapter : Microsoft.Extensions.Logging.ILogger
	{
		string _caller;
		public HostLogAdapter(string caller)
		{
			_caller = caller;
		}
		public IDisposable BeginScope<TState>(TState state) where TState : notnull => null;

		public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;

		public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			var message = formatter(state, exception);
			switch (logLevel)
			{
				case Microsoft.Extensions.Logging.LogLevel.Trace:
				case Microsoft.Extensions.Logging.LogLevel.None:
				case Microsoft.Extensions.Logging.LogLevel.Debug:
                case Microsoft.Extensions.Logging.LogLevel.Information:
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Warning:
					ServiceLocator.Resolve<ILogger>().Warning(message, _caller);
					break;
				case Microsoft.Extensions.Logging.LogLevel.Error:
					ServiceLocator.Resolve<ILogger>().Error(message, _caller, exception);
					break;
				case Microsoft.Extensions.Logging.LogLevel.Critical:
					ServiceLocator.Resolve<ILogger>().Fatal(message, _caller, exception);
					break;
			}
		}
	}
}

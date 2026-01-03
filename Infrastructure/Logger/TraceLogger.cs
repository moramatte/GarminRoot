using Infrastructure.Logger.Enterprise;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Infrastructure.EventAggregator;

namespace Infrastructure.Logger
{
	public abstract class TraceLogger : LoggerBase
	{
		protected readonly List<ITraceListener> _listeners = [];
		private int _logIdCount;

		protected override void Write(object sourceObj, LogLevel level, string message)
		{
			_logIdCount++;
			var logEntry = new LogEntry()
			{
				SourceObj = sourceObj,
				Level = level,
				Message = message,
				EventId = _logIdCount,
				ThreadId = Environment.CurrentManagedThreadId,
				Timestamp = Stamp.Now().Stamp
            };

			if (sourceObj is ILogIdentifiable logIdentifiable)
			{
				logEntry.Source = logIdentifiable.Name;
			}
            else
            {
				logEntry.Source = sourceObj?.ToString() ?? string.Empty;
            }

            Write(logEntry);
		}

		public void AddListeners(params ITraceListener[] listeners)
		{
			_listeners.AddRange(listeners);
		}

		public void RemoveListener(ITraceListener listeners)
		{
			_listeners.Remove(listeners);
		}

		public void AddSingletonListener(ITraceListener listener)
		{
			if (_listeners.Any(x => x.GetType() == listener.GetType()))
				return;
			AddListeners(listener);
		}

		public override void Flush()
		{
			foreach (var listener in _listeners)
			{
				listener.Flush();
			}
		}

		public override void Dispose()
		{
			base.Dispose();
			foreach (var listener in _listeners)
			{
				listener.Dispose();
			}
		}

		public T GetTraceListener<T>() where T : class => _listeners.FirstOrDefault(x => x is T) as T;

		protected virtual void Write(LogEntry logEntry)
		{
			//Extreme simplification of what Enterprise library does
			foreach (var listener in _listeners)
			{
				try
				{
					listener.Write(new TraceEventCache(), logEntry.Source, logEntry.Severity, logEntry.EventId, logEntry);
					listener.Flush();
				}
				catch (Exception e)
				{
					EmergencyLogger.Log(e.ToString());
				}
			}
		}
	}
}

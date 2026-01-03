using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awos7.WindowsService.LogConfig;
using Infrastructure.Logger.Enterprise;

namespace Infrastructure.Logger
{
	public class LogObjectTraceListener : CustomTraceListener
	{
		private readonly object _logObjectSyncLock = new();
		private List<LogObject> _logObjects = new();
		private const int _maxObj = 100;

		public LogObjectTraceListener(TraceListenerConfig config) : this(config.GetFormatter())
		{
			Name = config.Name;
			Filter = config.GetFilter();
		}

		public override TraceListenerConfig ToConfig()
		{
			return this.CommonConfig() with
			{
				Type = TraceListeners.LogObject
			};
		}

		public LogObjectTraceListener(ILogFormatter formatter) : base(formatter) { }

		public override void Write(LogEntry le, string message)
		{
			var strings = message.Split([' '], StringSplitOptions.RemoveEmptyEntries);
			message = $"{strings[0]} {strings[1]} {string.Join(" ", strings.Skip(4))}";
			lock (_logObjectSyncLock)
			{
				if (_logObjects.Count > _maxObj)
					_logObjects.RemoveAt(0);
				_logObjects.Add(new LogObject(le.Level, DateTime.UtcNow, le.Source, message));
			}
		}

		public List<LogObject> GetLogObjects()
		{
			List<LogObject> logObjets;
			lock (_logObjectSyncLock)
			{
				logObjets = _logObjects;

				if (!logObjets.Any())
					return null;

				_logObjects = new List<LogObject>();
			}

			return logObjets;
		}

	}
}

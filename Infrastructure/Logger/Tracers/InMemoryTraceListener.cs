using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awos7.WindowsService.LogConfig;
using Infrastructure.Logger.Enterprise;

namespace Infrastructure.Logger
{
	public class InMemoryTraceListener : CustomTraceListener
	{
		private readonly struct MemoryLogEntry(LogLevel Level, string Message)
		{
			public readonly LogLevel Level = Level;
			public readonly string Message = Message;
		}

		private readonly Dictionary<string, List<string>> _logs = new();
		private readonly List<MemoryLogEntry> _allLogs = new();
		private readonly object _syncLock = new();
		private const int _allLogsLength = 50000;

		public InMemoryTraceListener(TraceListenerConfig config) : this(config.GetFormatter())
		{
			Name = config.Name;
			Filter = config.GetFilter();
		}

		public override TraceListenerConfig ToConfig()
		{
			return this.CommonConfig() with
			{
				Type = TraceListeners.InMemory
			};
		}

		public InMemoryTraceListener(ILogFormatter formatter) : base(formatter) { }

		public override void Write(LogEntry le, string message)
		{
			if (le == null)
				return;
		
			lock (_syncLock)
			{
				Add(le.Source, le.Level, message);
			}
		}

		private void Add(string logSource, LogLevel level, string message)
		{
			if (!_logs.ContainsKey(logSource))
			{
				_logs.Add(logSource, [message]);
			}
			else
			{
				List<string> list = _logs[logSource];
				list.Add(message);
				if (list.Count > 100)
					list.RemoveAt(0);
			}

			// Add to allLogs
			_allLogs.Add(new(level, message));
			if (_allLogs.Count > _allLogsLength)
			{
				_allLogs.RemoveAt(0);
			}
		}

		public string LatestMessage()
		{
			lock (_syncLock)
			{
				return _allLogs.Last().Message;
			}
		}

		public string GetBulkErrorOrCriticals()
		{
			StringBuilder sb = new();
			var logs = _logs.ToList();
			foreach (string s in from keyValuePair in logs where keyValuePair.Value != null from s in keyValuePair.Value select s)
			{
				if (s.Contains("Error") || s.Contains("Critical"))
					sb.AppendLine(s);
			}
			return sb.ToString();
		}

		public List<string> GetIds()
		{
			lock (_syncLock)
				return _logs.Keys.ToList();
		}

		public List<string> GetLogs(string id)
		{
			lock (_syncLock)
				return _logs.ContainsKey(id) ? _logs[id] : [$"No logs for {id}"];
		}

		public IList<string> GetAllLogs()
		{
			lock (_syncLock)
			{
				return _allLogs.Select(x => x.Message).ToList();
			}
		}

		public IList<string> GetLogsByLevel(LogLevel level)
		{
			lock (_syncLock)
			{
				return _allLogs.Where(x => x.Level >= level).Select(x => x.Message).ToList();
			}
		}

		public void Clear()
		{
			lock (_syncLock)
			{
				_allLogs.Clear();
			}
		}
	}
}

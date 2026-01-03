using System;
using System.Linq;
using System.Text;
using Infrastructure.Logger;
using Infrastructure.Logger.Formatters;

namespace InfrastructureTests.Logging
{
	public class InMemoryLogger : TraceLogger
	{
		private readonly InMemoryTraceListener _tracer;

		public InMemoryLogger()
		{
			var awosFormatter = new RawFormatter();
			_tracer = new InMemoryTraceListener(awosFormatter);
			_listeners.Add(_tracer);
			_listeners.Add(new CustomConsoleTraceListener(awosFormatter));
		}

		public string Latest()
		{
			return _tracer.LatestMessage();
		}

		public bool Contains(string msg)
		{
			return _tracer.GetAllLogs().Any(message => message.Contains(msg));
		}

		public bool Contains(string msg, LogLevel ofLevel)
		{
			return _tracer.GetLogsByLevel(ofLevel).Any(message => message.Contains(msg));
		}

		public bool Matches(Func<string, bool> match)
		{
			return _tracer.GetAllLogs().Any(match);
		}

		public int Count(string msg)
		{
			return _tracer.GetAllLogs().Count(message => message.Contains(msg));
		}

		public void Clear()
		{
			_tracer.Clear();
		}

		public void AssertContains(string message)
		{
			if (!Contains(message))
			{
				throw new Exception($"No log entry containing '{message}' found among {AllMessages()}");
			}
		}

		public void AssertContains(string message, LogLevel ofLevel)
		{
			if (_tracer.GetLogsByLevel(ofLevel).Contains(message))
			{
				throw new Exception($"No log entry containing '{message}' found among {AllMessages()}");
			}
		}

		public void AssertContains(LogLevel level)
		{
			if (_tracer.GetLogsByLevel(level).Count == 0)
			{
				throw new Exception($"No log entry containing '{level}' found among {AllMessages()}");
			}
		}

		public void AssertLacks(string message)
		{
			if (Contains(message))
			{
				throw new Exception($"No log entry containing '{message}' found");
			}
		}

		public string AllMessages()
		{
			var sb = new StringBuilder();
			foreach (var entry in _tracer.GetAllLogs())
			{
				sb.Append($"{entry}, ");
			}
			return sb.ToString();
		}

	}

}

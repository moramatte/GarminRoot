using System.Collections.Generic;

namespace Infrastructure.Logger
{
	public static class LogMemory
	{
		private static InMemoryTraceListener GetLogger() => (ServiceLocator.Resolve<ILogger>() as TraceLogger)?.GetTraceListener<InMemoryTraceListener>();
		private static LogObjectTraceListener GetObjLogger() => (ServiceLocator.Resolve<ILogger>() as TraceLogger)?.GetTraceListener<LogObjectTraceListener>();
		public static string GetBulkErrorOrCriticals() => GetLogger()?.GetBulkErrorOrCriticals() ?? string.Empty;
		public static List<string> GetIds() => GetLogger()?.GetIds() ?? [];
		public static List<string> GetLogs(string id) => GetLogger()?.GetLogs(id) ?? [];
		public static IList<string> GetAllLogs() => GetLogger()?.GetAllLogs() ?? [];
		public static IList<string> GetLogsByLevel(LogLevel level) => GetLogger()?.GetLogsByLevel(level) ?? [];
		public static List<LogObject> GetLogObjects() => GetObjLogger()?.GetLogObjects() ?? [];
	}
}

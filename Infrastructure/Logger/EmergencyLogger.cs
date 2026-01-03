using System.Reflection;
using System.IO;
using System;
using Infrastructure.Extensions;

namespace Infrastructure.Logger
{
	public static class EmergencyLogger
	{
		private static string _logPath;
		public static string LogPath
        {
            set => _logPath = Path.Combine(Path.GetDirectoryName(value), LogFileName);
        }

		static string LogFileName => $"crashLog_{DateTime.UtcNow:d}_{DateTime.UtcNow:t}_{Guid.NewGuid()}.txt";

        static string FallbackFilePath
		{
			get
			{
				var strExeFilePath = string.Empty;
				try
				{
					strExeFilePath = Assembly.GetExecutingAssembly().Location;
				}
				catch { }
				if (string.IsNullOrEmpty(strExeFilePath))
					strExeFilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				var dir = Path.GetDirectoryName(strExeFilePath);
				return Path.Combine(dir, LogFileName);
			}
		}

        public static void Log(string message)
		{
			if (_logPath.HasContent())
			{
				try
				{
					File.AppendAllLines(_logPath, [message]);
				}
				catch 
				{
					FallbackLog(message);
				}
			}
			else
			{
				FallbackLog(message);
			}
		}

		private static void FallbackLog(string message)
		{
			try
			{
				File.AppendAllLines(FallbackFilePath, [message]);
			} catch { }
        }

	}
}

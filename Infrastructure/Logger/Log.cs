using System;
using System.Runtime.CompilerServices;

namespace Infrastructure.Logger
{
    public class Log
    {
        public static void Fatal(ILogIdentifiable caller, string message)
        {
            ServiceLocator.Resolve<ILogger>().Fatal(message, caller);
        }

        public static void Fatal(Type caller, string message)
        {
            ServiceLocator.Resolve<ILogger>().Fatal(message, caller);
        }

        public static void Error(ILogIdentifiable caller, string message)
        {
            ServiceLocator.Resolve<ILogger>().Error(message, caller);
        }

        public static void Error(ILogIdentifiable caller, string message, Exception ex)
        {
            ServiceLocator.Resolve<ILogger>().Error(message, caller, ex);
        }

        public static void Error(Type caller, string message, Exception ex)
        {
            ServiceLocator.Resolve<ILogger>().Error(message, caller, ex);
        }

        public static void Error(string message, Exception ex, [CallerFilePath] string caller = "")
        {
            ServiceLocator.Resolve<ILogger>().Error(message, caller, ex);
        }

        public static void Error(Type caller, string message)
        {
            ServiceLocator.Resolve<ILogger>().Error(message, caller);
        }

        public static void Warning(ILogIdentifiable caller, string message)
        {
            ServiceLocator.Resolve<ILogger>().Warning(message, caller);
        }
        
        public static void Warning(Type caller, string message)
        {
            ServiceLocator.Resolve<ILogger>().Warning(message, caller);
        }

        public static void Debug(ILogIdentifiable caller, string message)
        {
            ServiceLocator.Resolve<ILogger>().Debug(message, caller);
        }

        public static void Debug(Type caller, string message)
        {
            ServiceLocator.Resolve<ILogger>().Debug(message, caller);
        }

        public static void Info(ILogIdentifiable caller, string message)
        {
            ServiceLocator.Resolve<ILogger>().Info(message, caller);
        }

        public static void Info(Type caller, string message)
        {
            ServiceLocator.Resolve<ILogger>().Info(message, caller);
        }

        public static void Info(string message, [CallerFilePath] string caller = "")
        {
            ServiceLocator.Resolve<ILogger>().Info(message, caller);
        }

        public static void Flush()
        {
			ServiceLocator.Resolve<ILogger>().Flush();
		}
    }
}

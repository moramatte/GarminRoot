using Infrastructure.Logger;
using InfrastructureTests.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Extensions
{
    public static class TaskExtensions
    {
        public static AsyncDuring During(this Task task) 
        {
            return new AsyncDuring(task);
        }

        /// <summary>
        /// Use this to indicate a started fire-and-forget task
        /// </summary>
        /// <param name="task"></param>
        public static void FireAndForget(this Task task) 
        {
            task.ContinueWith(LogTaskError, TaskContinuationOptions.OnlyOnFaulted);
        }

        /// <summary>
        /// Use this to indicate a started fire-and-forget task
        /// </summary>
        /// <param name="task"></param>
        public static void FireAndForget(this ValueTask task)
        {
            task.AsTask().FireAndForget();
        }

        private static void LogTaskError(Task task)
        {
			if (ServiceLocator.HasRegistration<ILogger>())
	    		ServiceLocator.Resolve<ILogger>()?.Error("Fire and forget task threw exception", task.Exception);
        }
    }
}

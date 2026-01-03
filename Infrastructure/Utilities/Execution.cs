using System;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Utilities
{
    public static class Execution
    {
        private static bool? isRunningFromTest = null;

        /// <summary>
        /// Used to determine whether or not we are executing via a test runner.
        /// </summary>
        private static readonly List<string> KnownTestAssemblies = new List<string>()
        {
            "Microsoft.VisualStudio.TestPlatform.TestFramework",
			"testhost.x86",
		};

        /// <summary>
        /// Returns true if we are executing from a test runner.
        /// </summary>
        /// <returns>True if we are executing from a test runner, otherwise false</returns>
        public static bool IsRunningFromTest()
        {
            if (isRunningFromTest == null)
            {
                isRunningFromTest = AppDomain.CurrentDomain.GetAssemblies().Any(a => KnownTestAssemblies.Any(k => a.FullName.StartsWith(k)));
            }

            return isRunningFromTest.Value;
        }

        public static T IfRunningProd<T>(Func<T> obj) where T : class
        {
	        if (IsRunningFromTest())
	        {
		        return null;
	        }

	        return obj();
        }

		public static bool IsRunningProduction()
        {
            return !IsRunningFromTest();
        }

        /// <summary>
        /// When we are running from a test, this returns the first known test assembly loaded.
        /// </summary>
        public static string CurrentTestAssembly
        {
            get
            {
                return AppDomain.CurrentDomain.GetAssemblies().First(a => KnownTestAssemblies.Any(k => a.FullName.StartsWith(k))).FullName;
            }
        }
    }
}
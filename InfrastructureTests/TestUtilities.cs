using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Threading;
using FakeItEasy;
using Infrastructure;
using Infrastructure.EventAggregator;
using Infrastructure.Extensions;
using Infrastructure.Logger;

using Infrastructure.Threading;
using InfrastructureTests.Logging;
using InfrastructureTests.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Testably.Abstractions;
using Testably.Abstractions.Testing;
using Testably.Abstractions.TimeSystem;

namespace InfrastructureTests
{
    public static class InMyTest
    {
        public static IEnumerable<string> Events => GetAllEvents().Select(e => e.ToString());

        /// <summary>
        /// Override registration with a specified object instance
        /// </summary>
        /// <remarks>
        /// Previously resolved instance is disposed before overriding registration for the specified type.
        /// </remarks>
        /// <typeparam name="T">Type/Interface that is to be used for resolving the dependency.</typeparam>
        /// <param name="instanceObject">Instance that shall be returned at resolve</param>
        public static void OverrideSingleton<T>(T instanceObject) where T : class
        {
            RemoveRegistration<T>();
            ServiceLocator.RegisterSingleton<T>(instanceObject);
        }

        public static void OverrideSingleton<I, T>()  where T : I
        {
            RemoveRegistration<I>();
            ServiceLocator.RegisterSingleton<I, T>();
        }

        private static void RemoveRegistration<I>() 
        {
            if (ServiceLocator.HasRegistration<I>())
            {
                try
                {
                    if (ServiceLocator.RequiresDispose<I>())
                    {
                        ((IDisposable)ServiceLocator.Resolve<I>()).Dispose();
                    }
                }
                catch
                {
                    // Ignore any errors during resolve/dispose. Resolve will e.g. fail for types that require ResolveWith.
                }
            }
        }

        public static IFileSystem PopulateFileSystemFromEmbeddedResources(string resourcePrefix, string fakedFilePath, Assembly explicitAssembly = null)
        {
            var assembly = explicitAssembly != null ? explicitAssembly : Assembly.GetCallingAssembly();
            var assemblyName = assembly.GetName().Name;
            if (!resourcePrefix.StartsWith(assemblyName))
            {
                resourcePrefix = $"{assemblyName}.{resourcePrefix}";
            }

            var fakeFileSystem = new MockFileSystem(); 

            var allResources = assembly.GetManifestResourceNames();
            var allOurResources = allResources.Where(r => r.StartsWith(resourcePrefix));

            foreach (var resourcePath in allOurResources)
            {
				using var stream = assembly.GetManifestResourceStream(resourcePath);
				using var reader = new StreamReader(stream);
				var fileContent = reader.ReadToEnd();
				var path = $"{fakedFilePath}{SlashifyPath(resourcePath, resourcePrefix)}";
                fakeFileSystem.Directory.CreateDirectory(Path.GetDirectoryName(path));
				fakeFileSystem.File.WriteAllText(path, fileContent);
			}

            ServiceLocator.RegisterSingleton<IFileSystem>(fakeFileSystem);
            return fakeFileSystem;
        }

		public static IFileSystem PopulateFileSystemWithFiles(params (string filePath, string filedata)[] files)
		{
            var fakeFileSystem = ServiceLocator.Resolve<IFileSystem>() as MockFileSystem;
			fakeFileSystem ??= new MockFileSystem();
            
			foreach (var (filePath, filedata) in files)
			{
                fakeFileSystem.Directory.CreateDirectory(Path.GetDirectoryName(filePath));
				fakeFileSystem.File.WriteAllText(filePath, filedata);
			}

			ServiceLocator.RegisterSingleton<IFileSystem>(fakeFileSystem);
			return fakeFileSystem;
		}

		public static InMemoryLogger UseInMemoryLogger()
        {
            var logger = new InMemoryLogger();
            OverrideSingleton<ILogger>(logger);
            return logger;
        }

        public static HeadlessMessageboxProvider UseHeadlessMessageBoxes()
        {
            var boxes = new HeadlessMessageboxProvider();
            OverrideSingleton<IMessageBoxProvider>(boxes);
            return boxes;
        }

        public static bool QuestionWasAsked(string question)
        {
            var provider = ServiceLocator.Resolve<IMessageBoxProvider>() as HeadlessMessageboxProvider;
            return provider.DeliveredAnswers.Any(a => a.Item1.Contains(question));
        }

        public static void Responds(this string question, string answer)
        {
            var headlessProvider = ServiceLocator.Resolve<IMessageBoxProvider>() as HeadlessMessageboxProvider;
            headlessProvider.AnswerFor(question, answer);
        }

        private static string SlashifyPath(string dottedPath, string exclusionPrefix)
        {
            var strippedFromPrefix = dottedPath.Replace(exclusionPrefix, string.Empty);
            var withSlashes = strippedFromPrefix.Replace(".", "/");

            var lastSlash = withSlashes.LastIndexOf("/");
            if (lastSlash > 0)
            {
                withSlashes = withSlashes.Remove(lastSlash, 1).Insert(lastSlash, ".");
            }

            return withSlashes;
        }

        public static void RememberAllEvents()
        {
            var testReceiverPocoEventAggregator = new TestUtilities.TestablePocoEventAggregator();
            ServiceLocator.RegisterInstance<IPocoEventAggregator>(testReceiverPocoEventAggregator);
        }

        public static void ClearAllEvents()
        {
            var pocoAggregator = ServiceLocator.Resolve<IPocoEventAggregator>() as TestUtilities.TestablePocoEventAggregator;
            pocoAggregator.Events.Clear();
		}

		public static IEnumerable<object> GetAllEvents()
        {
            var aggregator = ServiceLocator.Resolve<IPocoEventAggregator>() as TestUtilities.TestablePocoEventAggregator;
            return aggregator.Events;
        }

        public static IFileSystem UseFakeFileFystem()
        {
            var fileSystem = new MockFileSystem((x) => x.UseTimeSystem(new RealTimeSystem()));
			ServiceLocator.RegisterSingleton<IFileSystem>(fileSystem);
            return ServiceLocator.Resolve<IFileSystem>();
        }

        public static IFileSystem UseFakeFileFystem(Dictionary<string, string> mockFiles)
        {
            var fileSystem = new MockFileSystem();
            foreach (var file in mockFiles)
            {
                fileSystem.File.WriteAllText(file.Key, file.Value);
            }
			ServiceLocator.RegisterSingleton<IFileSystem>(fileSystem);
            return ServiceLocator.Resolve<IFileSystem>();
        }

        public static IAsyncSchedulerMock ExtractScheduler(object instance, string timerMethodName = "")
        {
            return (ServiceLocator.Resolve<IAsyncSchedulerFactory>() as AsyncSchedulerMockFactory).FindScheduler(timerMethodName, instance);
        }

        public static void UseRealTimers()
        {
            ServiceLocator.RegisterSingleton<IAsyncSchedulerFactory, AsyncSchedulerFactory>();
        }

        public static void DiscloseEvents()
        {
            var eventList = Events.ToLinebreakSeparatedString();
            Console.WriteLine(eventList);
        }

        public static void DiscloseTopLevelEvents()
        {
            Console.WriteLine("Top Level events:");
			var eventList = Events.Where(e => e.HasAttribute<TopLevelEventAttribute>()).ToLinebreakSeparatedString();
            Console.WriteLine(eventList);
        }

        public static T RegisterFake<T>() where T : class
        {
            var fake = A.Fake<T>();
            ServiceLocator.RegisterInstance<T>(fake);
            return fake;
        }
	}

    public static class Events
    {
        public static IEnumerable<T> Of<T>()
        {
            var allEvents = InMyTest.GetAllEvents();
            var selectedEvents = allEvents.Where(ev => ev.GetType() == typeof(T)).ToList();
            var result = new List<T>();
            selectedEvents.ForEach(ev => result.Add((T)ev));
            return result;
        }

        public static void Assert<T>(Func<T, bool> predicate)
        {
            var selectedEvents = Of<T>();
            if (selectedEvents.None(ev => predicate(ev)))
            {
                throw new AssertFailedException();
            }
        }

        public static void AssertLacks<T>(Func<T, bool> predicate)
        {
            var selectedEvents = Of<T>();
            if (selectedEvents.Any(ev => predicate(ev)))
            {
                throw new AssertFailedException();
            }
        }
	}

    public static class EventCount
    {
        public static int Of<T>()
        {
            var selectedEvents = Events.Of<T>();
            return selectedEvents.Count();
        }
    }

    /// <summary>
    /// Convenience methods for testing
    /// </summary>
    public static class TestUtilities
    {
        public static bool IsLiveUnitTest
        {
            get
            {
                string currentPath = Directory.GetCurrentDirectory();
                if (currentPath.Contains("Users") || currentPath.Contains("lut"))
                {
                    return true;
                }
                return false;
            }
        }


        /// <summary>
        /// Gets a random value from a given enum
        /// </summary>
        /// <typeparam name="T">The enum type</typeparam>
        /// <returns>A value of T</returns>
        public static T RandomEnumValue<T>()
        {
            var values = Enum.GetValues(typeof(T));
            var random = new Random();
            var randomValue = (T)values.GetValue(random.Next(values.Length));
            return randomValue;
        }
        
        /// <summary>
        /// Wait for a condition to be true.
        /// </summary>
        /// <param name="waitCondition">Condition to check</param>
        /// <param name="wait">Waiting time, default 10s</param>
        /// <param name="message">Message in assert exception</param>
        public static void WaitForCondition(Func<bool> waitCondition, TimeSpan wait = default(TimeSpan), string message = "Timeout")
        {
            if (wait == default(TimeSpan))
            {
                wait = TimeSpan.FromSeconds(10);
            }

            var timeOutTime = DateTime.Now + wait;

            while (true)
            {
                if (waitCondition())
                {
                    return;
                }

                if (DateTime.Now > timeOutTime)
                {
                    Assert.Fail(message);
                }

                Thread.Sleep(50);
            }
        }

        /// <summary>
        /// Wait for a condition to be true.
        /// </summary>
        /// <param name="waitCondition">Condition to check</param>
        /// <param name="message">Message in assert exception</param>
        public static void WaitForCondition(Func<bool> waitCondition, string message)
        {
            WaitForCondition(waitCondition, default(TimeSpan), message);
        }

        public class TestablePocoEventAggregator : PocoEventAggregator
        {
            public List<object> Events { get; set; } = new();

            public override void Publish(object message)
            {
                base.Publish(message);
                Events.Add(message);
            }
        }
    }
}

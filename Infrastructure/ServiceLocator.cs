using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Infrastructure.EventAggregator;
using Infrastructure.Extensions;
using Infrastructure.Logger;
using Infrastructure.Logger.Formatters;
using Infrastructure.Logger.Tracers;
using Infrastructure.Network;
using Infrastructure.Threading;
using Infrastructure.Validation;
using Unity;
using Unity.Lifetime;
using System.IO.Abstractions;
using Testably.Abstractions;

namespace Infrastructure
{
    public static class ServiceLocator
    {
        private static Lazy<UnityContainer> container = new(SpinUp);
 
        public static UnityContainer Container => container.Value;
        public static bool IsDisposing = false;

        private static UnityContainer SpinUp()
        {
            var ioc = new UnityContainer();
            ioc.AddExtension(new SingletonTrackerExtension());

            RegisterMandatoryComponents(ioc);
            return ioc;
        }

        public static void Reset()
        {
            Resolve<ILogger>().Info("ServiceLocator resetting");

            DisposeSingletons();

            container = new Lazy<UnityContainer>(SpinUp);
        }

        private static void DisposeSingletons()
        {
            IsDisposing = true;
            var singletonRepo = Container.Configure<SingletonTrackerExtension>();
            var singletonList = singletonRepo.GetSingletons().OrderBy(item => item is ILogger).ToList();
            
            foreach (var singleton in singletonList)
            {
                if (singleton is IUnityContainer)
                {
                    continue;
                }

                try
                {
                    if (singleton is IDisposable disposableSingleton)
                    {
                        disposableSingleton.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Failure disposing {singleton}", ex);
                } 
            }
            IsDisposing = false;
        }

        public static void Register<T, U>() where U : T
        {
            Container.RegisterType<T, U>();
        }

        public static void RegisterSingleton<T, U>() where U : T
        {
            Container.RegisterType<T, U>(AsSingleton());
        }

        public static void RegisterTransient<T, U>() where U : T
        {
            Container.RegisterType<T, U>(new TransientLifetimeManager());
        }

        public static void RegisterTransient<T>() 
        {
	        Container.RegisterType<T, T>(new TransientLifetimeManager());
        }

		public static void RegisterSingleton<T>(Type implementingType) 
        {
            Container.RegisterType(typeof(T), implementingType, AsSingleton());
        }

        public static void RegisterSingleton<T>(T instance)
        {
            Container.RegisterInstance<T>(instance);
        }

        public static void RegisterSingleton<T>() where T : class
        {
            Container.RegisterSingleton<T>();
        }

        [DebuggerStepThrough]
        public static T Resolve<T>()
        {
            var instance = Container.Resolve<T>();
            return instance;
        }

        [DebuggerStepThrough]
        public static T Resolve<T>(string name)
        {
            var instance = Container.Resolve<T>(name);
            return instance;
        }

        public static IEnumerable<T> GetAllOfType<T, TSource>() where T : class
        {
            var identifiables = Container.ResolveAll<TSource>();

            var query = from identifiable in identifiables
                where identifiable is T
                select identifiable as T;
            return query;
        }

        private static void RegisterMandatoryComponents(UnityContainer ioc)
        {
            ioc.RegisterInstance(new GlobalCancellationToken(new()));
            ioc.RegisterType<IAssemblyStore, AssemblyStore>(AsSingleton());
            ioc.RegisterType<ITimeProvider, TimeProvider>(AsSingleton());
            ioc.RegisterType<IPocoEventAggregator, PocoEventAggregator>(AsSingleton());
			ioc.RegisterType<IAsyncWorkerFactory, AsyncWorkerFactory>(AsSingleton());
			ioc.RegisterType<ILogger, DefaultLogger>(AsSingleton());
            ioc.RegisterType<IFileSystem, RealFileSystem>(AsSingleton());
            ioc.RegisterType<IDns, DnsWrapper>();
            ioc.RegisterType<Validator>(AsSingleton());
            ioc.RegisterType<XmlSerializerRepository>(AsSingleton());
            ioc.RegisterType<IAsyncSchedulerFactory, AsyncSchedulerFactory>(AsSingleton());
        }

        public static ContainerControlledLifetimeManager AsSingleton()
        {
            return new ContainerControlledLifetimeManager();
        }

        public static void RegisterInstance<T>(T instance)
        {
            Container.RegisterInstance(typeof(T), instance, AsSingleton());
        }

		public static void RegisterInstance<T>(string name, T instance)
        {
            Container.RegisterInstance<T>(name, instance, AsSingleton());
        }

        public static void BuildUp(object instance)
        {
            var t = instance.GetType();
            var result = Container.BuildUp(t, instance);
        }

        public static bool TryResolve<T>(out T t)
        {
            if (HasRegistration<T>())
            {
                t = Resolve<T>();
                return true;
            }

            t = default;
            return false;
        }

        public static bool HasRegistration<T>()
        {
            return Container.Registrations.Any(r => r.RegisteredType == typeof(T));
        }

        public static bool HasRegistration<T>(string name)
        {
            return Container.Registrations.Any(r => r.RegisteredType == typeof(T) && r.Name == name);
        }

        internal static bool HasRegistration(Type t)
        {
            return Container.Registrations.Any(r => r.RegisteredType == t);
        }
        
        public static void RegisterLogger<IL, T1>() where T1 : IL where IL : ILogger
        {
            container.Value.RegisterType<IL, T1>(AsSingleton());
        }

        public static bool RequiresDispose<I>()
        {
	        var singletonRepo = container.Value.Configure<SingletonTrackerExtension>();
	        return singletonRepo.GetSingletons().Any(s => s is I && s is IDisposable);
        }

        public static void LogToSimpleFile(string logFilePath)
        {
            var logger = Resolve<ILogger>() as DefaultLogger;
            if (!Path.IsPathRooted(logFilePath))
            {
                logFilePath = Path.Combine(ConfigFile.Folder, $"Logs\\{logFilePath}");
            }
			var log4Net = new Log4NetFileTraceListener(new RawFormatter(), logFilePath, nameof(Log4NetFileTraceListener));
            logger.AddListeners(log4Net);
		}

        public static void LogToEvents()
        {
            var logger = Resolve<ILogger>();
            if (logger is TraceLogger traceLogger)
                traceLogger.AddSingletonListener(new EventTraceListener(new RawFormatter()));
            else
                throw new InvalidOperationException("The used logger must support trace listeners");
        }
    }

    public record GlobalCancellationToken(CancellationTokenSource Token);
}

using System;
using System.Collections.Generic;
using Infrastructure.Logger;
using Unity.Builder;
using Unity.Extension;
using Unity.Lifetime;
using Unity.Strategies;

namespace Infrastructure.Extensions
{
	public class SingletonTrackerExtension : UnityContainerExtension
	{
		private readonly HashSet<object> singletons = new HashSet<object>();

		protected override void Initialize()
		{
			Context.Strategies.Add(new SingletonTrackingStrategy(this), UnityBuildStage.PreCreation);

			Context.RegisteringInstance += (sender, e) =>
			{
                if (ServiceLocator.IsDisposing)
                {
					ServiceLocator.Resolve<ILogger>().Warning($"Created singleton {e.Instance} during dispose");
                }

				if (e.LifetimeManager is ContainerControlledLifetimeManager)
				{
					singletons.Add(e.Instance);
                }
			};
		}

		public void TrackInstance(object instance)
		{
            if (ServiceLocator.IsDisposing)
            {
                ServiceLocator.Resolve<ILogger>().Warning($"Created singleton {instance} during dispose");
            }

            if (!singletons.Contains(instance))
			{
				singletons.Add(instance);
            }
		}

		public IEnumerable<object> GetSingletons()
		{
			return singletons;
		}
	}

	public class SingletonTrackingStrategy : BuilderStrategy
	{
		private readonly SingletonTrackerExtension singletonRepo;

		public SingletonTrackingStrategy(SingletonTrackerExtension extension)
		{
			singletonRepo = extension;
		}

		public override void PostBuildUp(ref BuilderContext context)
		{
			if (context.RequiresRecovery is ContainerControlledLifetimeManager &&
			    context.Existing != null)
			{
				singletonRepo.TrackInstance(context.Existing);
			}
		}
	}
}

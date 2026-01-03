using System;
using System.Threading.Tasks;

namespace Infrastructure.EventAggregator
{
    /// <summary>
    /// Convenience class that simplifies the ServiceLocator.Resolve<IEventAggregator>().Subscribe harang
    /// </summary>
    public static class Subscribe
    {
        public static void To<T>(Action<T> eventType)
        {
            ServiceLocator.Resolve<IPocoEventAggregator>().Subscribe(eventType);
		}

		public static void To<T>(Func<T, Task> eventType)
		{
			ServiceLocator.Resolve<IPocoEventAggregator>().Subscribe(eventType);
		}

		public static void UnsubscribeFromEvents(this object subscriber)
        {
            ServiceLocator.Resolve<IPocoEventAggregator>().Unsubscribe(subscriber);
        }
    }
}

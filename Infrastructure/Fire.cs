using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Infrastructure.EventAggregator;
using Infrastructure.Extensions;
using Prism.Events;

namespace Infrastructure
{
    public static class Fire
    {
        public static void Event(object payload)
        {
            ServiceLocator.Resolve<IPocoEventAggregator>().Publish(payload);
        }

        public static void Event<T>() where T : new()
        {
            var myEvent = new T();
            ServiceLocator.Resolve<IPocoEventAggregator>().Publish(myEvent);
        }

        public static void Event<T>(T payload)
        {
            ServiceLocator.Resolve<IPocoEventAggregator>().Publish(payload);
        }

		public static Task EventAsync<T>(T payload)
		{
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("Browser")))
            {
                throw new NotSupportedException();
            }

            return ServiceLocator.Resolve<IPocoEventAggregator>().PublishAsync(payload);
		}
	}

    public static class Collect
    {
        public static void Payload<T>(T payload)
        {
            Fire.Event<T>(payload);
        }
    }
}

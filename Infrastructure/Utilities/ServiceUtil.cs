using System;
using System.ServiceProcess;
using Infrastructure.Logger;

namespace Infrastructure.Utilities
{
	public static class ServiceUtil
	{
        public static void StartService(string name)
        {
            var service = new ServiceController(name);
            try
            {
                var timeout = TimeSpan.FromMilliseconds(30000);

                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);

            }
            catch (Exception exc)
            {
               Log.Error(typeof(ServiceUtil), $"Error starting service: {name}", exc);
            }
		}

        public static void StopService(string name)
        {
            var service = new ServiceController(name);
            try
            {
                var timeout = TimeSpan.FromMilliseconds(30000);

                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);

            }
            catch (Exception exc)
            {
                Log.Error(typeof(ServiceUtil), $"Error Stopping service: {name}", exc);
            }
        }
	}
}

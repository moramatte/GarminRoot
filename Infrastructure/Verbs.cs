using Infrastructure.Utilities;

namespace Infrastructure
{
	public static class Start
	{
        public static void Service(string name)
        {
			ServiceUtil.StartService(name);
        }
	}

    public static class Stop
    {
        public static void Service(string name)
        {
            ServiceUtil.StopService(name);
        }
    }

    public static class Mount
    {
        public static void Drive(string driveName, string path, string username, string password)
        {
            DriveUtil.Mount(driveName, path, username, password);
        }
    }
}

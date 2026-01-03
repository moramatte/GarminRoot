using System.Diagnostics;
using Infrastructure.Logger;

namespace Infrastructure.Utilities
{
	public static class DriveUtil
	{
        public static void Mount(string driveName, string path, string username, string password)
        {
			var args = $"use {driveName} {path} /user:{username} {password}";
			var p = new Process();
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.FileName = "net";
			p.StartInfo.Arguments = $" {args}";
			p.StartInfo.CreateNoWindow = true;
			p.Start();
			p.WaitForExit(3000);			

			var output = p.StandardOutput.ReadToEnd();
			p.Dispose();

			Log.Info($"Mount result: {output}");			
		}

        public static void Disconnect(string driveName)
        {
            var args = $"use {driveName} /delete /yes";
			var p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "net";
            p.StartInfo.Arguments = $" {args}";
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            p.WaitForExit(3000);
            var output = p.StandardOutput.ReadToEnd();
            p.Dispose();

            Log.Info($"Unmount result: {output}");		
		}
	}
}

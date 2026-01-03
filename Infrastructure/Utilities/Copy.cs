using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Infrastructure.Logger;

namespace Infrastructure.Utilities
{
	public static class Copy
	{
        public static void Recursively(string sourcePath, string targetPath)
        {
			var startTime = DateTime.Now;

			var fs = ServiceLocator.Resolve<IFileSystem>();

            var directories = fs.Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories);
            var directoryCount = directories.Length;

            Log.Info(typeof(Copy), $"{directoryCount} directories to be copied...");

			for (int i = 0; i < directoryCount; i++)
            {
                var dirPath = directories[i];
				try
				{                  
                    fs.Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
                }
                catch (Exception e)
                {
                    Log.Error(typeof(Copy), $"Error copying directory {i}/{directoryCount} : {dirPath}", e);
                }
			}
			
            var files = fs.Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
            var fileCount = files.Length;

            Log.Info(typeof(Copy), $"{files.Length} files to be copied...");
            
            Parallel.ForEach(files, (currentFile) =>
            {
                try
                {
				    fs.File.Copy(currentFile, currentFile.Replace(sourcePath, targetPath), true);
					Log.Info(typeof(Copy), $"Copied file {currentFile}");
				}
                catch (Exception e) 
                {
                    Log.Error(typeof(Copy), $"Failed to copy file {currentFile}", e);
                }
			});

            var elapsed = DateTime.Now - startTime;

            Log.Info(typeof(Copy), $"Copy operation completed. Took {elapsed.TotalSeconds} seconds.");
		}       
	}
}

using System;
using System.IO;
using System.IO.Abstractions;
using Infrastructure.Logger;

namespace Infrastructure.Extensions
{
	public static class IO
	{
        public static IFileSystem FileSystem => ServiceLocator.Resolve<IFileSystem>();

		public static void Clear(string directoryPath)
        {
            var fs = FileSystem;
            fs.Directory.Delete(directoryPath, true);
        }

		public static class FileStream
		{
			public static FileSystemStream New(string path, FileMode mode, FileAccess access)
			{
				return FileSystem.FileStream.New(path, mode, access);
			}
		}

        public static void MoveFile(string sourcePath, string targetPath)
        {
            try
            {
                FileSystem.File.Move(sourcePath, targetPath);
            }
            catch (Exception e)
            {
                Log.Error($"Error moving file from {sourcePath} to {targetPath}", e);
                throw;
            }
        }

        public static bool FileExists(string path)
        {
            var fs = FileSystem;
            return fs.File.Exists(path);
        }

        public static class Path
        {
		    public static string Combine(params string[] paths)
		    {
			    var fs = FileSystem;
			    return fs.Path.Combine(paths);
		    }

			public static string GetDirectoryName(string path)
			{
				var fs = FileSystem;
				return fs.Path.GetDirectoryName(path);
			}

            public static bool IsPathRooted(string path)
            {
				var fs = FileSystem;
				return fs.Path.IsPathRooted(path);
			}
		}

		public static void DeleteFile(string path)
        {
            FileSystem.File.Delete(path);
        }

        public static DateTime FileCreationTimeUtc(string path)
        {
            return FileSystem.File.GetCreationTimeUtc(path);
        }

        public static string GetDirectoryName(string path)
        {
            return FileSystem.Path.GetDirectoryName(path);
        }

        public static bool DirectoryExists(string path)
        {
            return FileSystem.Directory.Exists(path);
        }

        public static IDirectoryInfo CreateDirectory(string path)
        {
            return FileSystem.Directory.CreateDirectory(path);
        }

        public static DateTime FileCreationTime(string path)
        {
            return FileSystem.File.GetCreationTime(path);
        }

        public static FileSystemStream OpenFile(string path, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            return FileSystem.File.Open(path, fileMode, fileAccess, fileShare);
        }

        public static string ReadAllText(string path)
		{
			return FileSystem.File.ReadAllText(path);
		}
        public static void Copy(string source, string destination, bool overwrite)
        {
            FileSystem.File.Copy(source, destination, overwrite);
        }

        public static void WriteAllText(string path, string contents)
        {
            if (Path.IsPathRooted(path)) 
            {
                try
                {
                    var directory = Path.GetDirectoryName(path);
                    if (!FileSystem.Directory.Exists(directory))
                    {
                        FileSystem.Directory.CreateDirectory(directory);
                    }
                }
                catch (Exception e)
                {
                    Log.Error(typeof(IO), "Error creating missing directory automatically.", e);
                    throw;
                }                
            }
            
            FileSystem.File.WriteAllText(path, contents);
        }
	}
}

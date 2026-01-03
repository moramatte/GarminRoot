using System.IO.Abstractions;
using Testably.Abstractions.Testing;

namespace InfrastructureTests.Extensions
{
	public static class FakeFileSystemExtensions
	{
		public static void AddDrive(this IFileSystem fileSystem, string path)
		{
			var f = fileSystem as MockFileSystem;
			f.WithDrive(path);
		}
	}
}

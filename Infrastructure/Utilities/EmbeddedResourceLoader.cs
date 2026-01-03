using System.IO;
using System.Reflection;

namespace Infrastructure.Utilities
{
    public static class Load
    {
        public static string EmbeddedResource(string path)
        {
            var assembly = Assembly.GetCallingAssembly();
            var assemblyName = assembly.GetName().Name;
           
            var allResources = assembly.GetManifestResourceNames();

            foreach (var resourcePath in allResources)
            {
                var simplePath = resourcePath;
                if (resourcePath.StartsWith(assemblyName))
                {
                    simplePath = resourcePath.Remove(0, assemblyName.Length + 1);
                }

                if (simplePath == path)
                {
                    using (var stream = assembly.GetManifestResourceStream(resourcePath))
                    using (var reader = new StreamReader(stream))
                    {
                        var fileContent = reader.ReadToEnd();
                        return fileContent;
                    }
                }
            }

            throw new FileNotFoundException($"Did not find embedded resource at {path}");
        }

        public static byte[] BytesFromEmbeddedResource(string path)
        {
            var assembly = Assembly.GetCallingAssembly();
            var assemblyName = assembly.GetName().Name;

            var allResources = assembly.GetManifestResourceNames();

            foreach (var resourcePath in allResources)
            {
                var simplePath = resourcePath;
                if (resourcePath.StartsWith(assemblyName))
                {
                    simplePath = resourcePath.Remove(0, assemblyName.Length + 1);
                }

                if (simplePath == path)
                {
                    using (var stream = assembly.GetManifestResourceStream(resourcePath))
                    using (var reader = new BinaryReader(stream))
                    {
                        var fileContent = reader.ReadBytes((int)stream.Length);
                        return fileContent;
                    }
                }
            }

            throw new FileNotFoundException($"Did not find embedded binary resource at {path}");
        }
	}
}

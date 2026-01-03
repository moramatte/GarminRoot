using System.IO;
using System.Text;

namespace Infrastructure.Extensions
{
    public static class StreamExtensions
    {
        public static string GetAllText(this MemoryStream theMemoryStream)
        {
            return Encoding.Default.GetString((theMemoryStream.ToArray()));
        }
    }
}

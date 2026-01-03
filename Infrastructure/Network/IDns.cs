using System.Net;
using System.Threading.Tasks;

namespace Infrastructure.Network;

public interface IDns
{
	Task<IPHostEntry> GetHostEntryAsync(string hostName);
}

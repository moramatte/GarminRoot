using System.Net;
using System.Threading.Tasks;

namespace Infrastructure.Network;

public class DnsWrapper : IDns
{
    public async Task<IPHostEntry> GetHostEntryAsync(string hostName)
    {
        return await Dns.GetHostEntryAsync(hostName);
    }
}

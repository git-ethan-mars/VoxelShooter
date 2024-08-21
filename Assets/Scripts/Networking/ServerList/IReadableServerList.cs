using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Networking.ServerList
{
    public interface IReadableServerList
    {
        List<ServerInfo> GetServersInfo();
        Task GetServers();
    }
}
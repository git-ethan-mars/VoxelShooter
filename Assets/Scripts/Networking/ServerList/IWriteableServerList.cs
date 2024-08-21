using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Networking.ServerList
{
    public interface IWriteableServerList
    {
        List<ServerInfo> GetServersInfo();
        Task UpdateServer(ServerInfo serverInfo);
        Task DeleteServer(ServerInfo serverInfo);
    }
}
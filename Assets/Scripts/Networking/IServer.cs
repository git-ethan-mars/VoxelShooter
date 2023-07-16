using MapLogic;
using Mirror;

namespace Networking
{
    public interface IServer
    {
        IMapProvider MapProvider { get; }
        ServerData ServerData { get; }
        IMapUpdater MapUpdater { get; }
        void AddKill(NetworkConnectionToClient source, NetworkConnectionToClient receiver);
    }
}
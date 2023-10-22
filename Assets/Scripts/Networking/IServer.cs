using Data;
using MapLogic;
using Mirror;
using Networking.ServerServices;
using Steamworks;

namespace Networking
{
    public interface IServer
    {
        MapProvider MapProvider { get; }
        ServerData ServerData { get; }
        MapUpdater MapUpdater { get; }
        void AddKill(NetworkConnectionToClient source, NetworkConnectionToClient receiver);

        void AddPlayer(NetworkConnectionToClient connection, GameClass chosenClass, CSteamID steamID,
            string nickname);

        void ChangeClass(NetworkConnectionToClient connection, GameClass chosenClass);
        void DeletePlayer(NetworkConnectionToClient connection);
        void Damage(NetworkConnectionToClient source, NetworkConnectionToClient receiver, int totalDamage);
        void SendMap(NetworkConnectionToClient connection);
        void Start();
        void Stop();
    }
}
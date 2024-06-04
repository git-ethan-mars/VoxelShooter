using System.Collections.Generic;
using Data;
using MapLogic;
using Mirror;
using Networking.ServerServices;
using Steamworks;

namespace Networking
{
    public interface IServer
    {
        IMapProvider MapProvider { get; }
        MapUpdater MapUpdater { get; }
        BlockHealthSystem BlockHealthSystem { get; }
        EntityContainer EntityContainer { get; }
        IEnumerable<NetworkConnectionToClient> ClientConnections { get; }
        string MapName { get; }

        void AddPlayer(NetworkConnectionToClient connection, CSteamID steamID,
            string nickname);

        void ChangeClass(NetworkConnectionToClient connection, GameClass chosenClass);
        void DeletePlayer(NetworkConnectionToClient connection);
        void Damage(NetworkConnectionToClient source, NetworkConnectionToClient receiver, int totalDamage);
        void Heal(NetworkConnectionToClient receiver, int totalHeal);
        void SendCurrentServerState(NetworkConnectionToClient connection);
        void Start();
        void Stop();
        bool TryGetPlayerData(NetworkConnectionToClient connection, out PlayerData playerData);
        PlayerData GetPlayerData(NetworkConnectionToClient connectionToClient);
    }
}
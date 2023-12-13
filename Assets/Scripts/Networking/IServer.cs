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
        ServerData Data { get; }
        BlockHealthSystem BlockHealthSystem { get; }

        void AddPlayer(NetworkConnectionToClient connection, CSteamID steamID,
            string nickname);

        void ChangeClass(NetworkConnectionToClient connection, GameClass chosenClass);
        void DeletePlayer(NetworkConnectionToClient connection);
        void Damage(NetworkConnectionToClient source, NetworkConnectionToClient receiver, int totalDamage);
        void Heal(NetworkConnectionToClient receiver, int totalHeal);
        void SendCurrentServerState(NetworkConnectionToClient connection);
        void Start();
        void Stop();
    }
}
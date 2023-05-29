using Data;
using Mirror;

namespace Infrastructure.Factory
{
    public interface IPlayerFactory
    {
        void CreatePlayer(NetworkConnectionToClient connection);
        void CreateSpectatorPlayer(NetworkConnectionToClient connection);
        void RespawnPlayer(NetworkConnectionToClient connection);
    }
}
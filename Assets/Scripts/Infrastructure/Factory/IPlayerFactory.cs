using Data;
using Infrastructure.Services;
using Mirror;

namespace Infrastructure.Factory
{
    public interface IPlayerFactory : IService
    {
        void CreatePlayer(NetworkConnectionToClient connection, GameClass gameClass, string nickName);
        void CreateSpectatorPlayer(NetworkConnectionToClient connection, GameClass gameClass);
    }
}
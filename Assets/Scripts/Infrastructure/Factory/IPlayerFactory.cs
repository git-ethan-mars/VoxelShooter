using Data;
using Infrastructure.Services;
using Mirror;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IPlayerFactory : IService
    {
        GameObject CreatePlayer(GameClass gameClass, string nickName);
        GameObject RespawnPlayer(NetworkConnectionToClient connection, GameClass gameClass);
    }
}
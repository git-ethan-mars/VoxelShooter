using Data;
using Infrastructure.Services;
using Mirror;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IEntityFactory : IService
    {
        GameObject CreatePlayer(NetworkConnectionToClient connection, GameClass gameClass, string nickName);
        GameObject RespawnPlayer(NetworkConnectionToClient connection, GameClass gameClass);
    }
}
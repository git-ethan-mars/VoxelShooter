using Data;
using Infrastructure.Services;
using Mirror;
using Networking;
using Networking.Messages;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IEntityFactory : IService
    {
        GameObject CreatePlayer(NetworkConnectionToClient conn, CharacterMessage message);
        GameObject RespawnPlayer(NetworkConnectionToClient connection, GameClass gameClass);
    }
}
using System;
using Infrastructure.Factory;
using Mirror;
using Networking.Messages;

namespace Networking.Synchronization
{
    public class HealthSynchronization : NetworkBehaviour
    {
        private ServerData _serverData;
        private IEntityFactory _entityFactory;

        public void Construct(ServerData serverData, IEntityFactory entityFactory)
        {
            _serverData = serverData;
            _entityFactory = entityFactory;
        }


        [Server]
        public void Damage(NetworkConnectionToClient connection, int totalDamage)
        {
            var playerData = _serverData.GetPlayerData(connection.connectionId);
            playerData.Health -= totalDamage;
            if (playerData.Health <= 0)
            {
                playerData.Health = 0;
                _serverData.UpdatePlayer(connection);
                var gameClass = _serverData.GetPlayerData(connection.connectionId).GameClass;
                var player = _entityFactory.RespawnPlayer(connection, gameClass);
                var oldPlayer = connection.identity.gameObject;
                NetworkServer.ReplacePlayerForConnection(connection, player, true);
                Destroy(oldPlayer, 0.1f);
            }
            else
            {
                connection.Send(new HealthMessage()
                    {CurrentHealth = playerData.Health, MaxHealth = playerData.MaxHealth});
            }
        }

        [Command]
        public void Die(NetworkConnectionToClient connection = null)
        {
            _serverData.UpdatePlayer(connection);
            var gameClass = _serverData.GetPlayerData(connection.connectionId).GameClass;
            var player = _entityFactory.RespawnPlayer(connection, gameClass); 
            var oldPlayer = connection.identity.gameObject;
            NetworkServer.ReplacePlayerForConnection(connection, player, true);
            Destroy(oldPlayer, 0.1f);
        }
    }
}
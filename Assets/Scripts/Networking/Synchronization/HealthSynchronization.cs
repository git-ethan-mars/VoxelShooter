using System;
using Infrastructure.Factory;
using Mirror;

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
            playerData.health -= totalDamage;
            connection.Send(new HealthMessage()
                {CurrentHealth = Math.Max(playerData.health, 0), MaxHealth = playerData.maxHealth});
            if (playerData.health <= 0)
            {
                var gameClass = _serverData.GetPlayerData(connection.connectionId).gameClass;
                var player = _entityFactory.RespawnPlayer(connection, gameClass);
                _serverData.UpdatePlayer();
                NetworkServer.ReplacePlayerForConnection(connection, player, true);

            }
        }
    }
}
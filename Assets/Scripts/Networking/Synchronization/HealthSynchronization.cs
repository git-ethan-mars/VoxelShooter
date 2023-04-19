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
            playerData.Health -= totalDamage;
            connection.Send(new HealthMessage()
                {CurrentHealth = Math.Max(playerData.Health, 0), MaxHealth = playerData.MaxHealth});
            if (playerData.Health <= 0)
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
}
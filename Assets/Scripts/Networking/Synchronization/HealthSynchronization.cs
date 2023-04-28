using Infrastructure.Factory;
using Mirror;
using PlayerLogic;

namespace Networking.Synchronization
{
    public class HealthSynchronization : NetworkBehaviour
    {
        private ServerData _serverData;
        private IPlayerFactory _playerFactory;

        public void Construct(ServerData serverData, IPlayerFactory playerFactory)
        {
            _serverData = serverData;
            _playerFactory = playerFactory;
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
                var player = _playerFactory.RespawnPlayer(connection, gameClass);
                var oldPlayer = connection.identity.gameObject;
                NetworkServer.ReplacePlayerForConnection(connection, player, true);
                Destroy(oldPlayer, 0.1f);
            }
            else
            {
                connection.identity.gameObject.GetComponent<HealthSystem>().health = playerData.Health; 
            }
        }

        [Command]
        public void Die(NetworkConnectionToClient connection = null)
        {
            _serverData.UpdatePlayer(connection);
            var gameClass = _serverData.GetPlayerData(connection.connectionId).GameClass;
            var player = _playerFactory.RespawnPlayer(connection, gameClass); 
            var oldPlayer = connection.identity.gameObject;
            NetworkServer.ReplacePlayerForConnection(connection, player, true);
            Destroy(oldPlayer, 0.1f);
        }
    }
}
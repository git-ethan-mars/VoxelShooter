using Infrastructure.Factory;
using Mirror;
using PlayerLogic;
using UnityEngine;

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
        public void Damage(NetworkConnectionToClient source, NetworkConnectionToClient receiver, int totalDamage)
        {
            var playerData = _serverData.GetPlayerData(receiver);
            playerData.Health -= totalDamage;
            if (playerData.Health <= 0)
            {
                playerData.Health = 0;
                _serverData.UpdatePlayer(receiver);
                _serverData.AddDeath(receiver);
                _serverData.AddKill(source);
                Debug.Log($"{source} killed {receiver}");
                var gameClass = playerData.GameClass;
                var player = _playerFactory.RespawnPlayer(receiver, gameClass);
                var oldPlayer = receiver.identity.gameObject;
                NetworkServer.ReplacePlayerForConnection(receiver, player, true);
                Destroy(oldPlayer, 0.1f);
            }
            else
            {
                receiver.identity.gameObject.GetComponent<HealthSystem>().health = playerData.Health;
            }
        }
    }
}
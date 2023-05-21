using Mirror;
using PlayerLogic;
using UnityEngine;

namespace Networking.Synchronization
{
    public class HealthSynchronization : NetworkBehaviour
    {
        private ServerData _serverData;

        public void Construct(ServerData serverData)
        {
            _serverData = serverData;
        }


        [Server]
        public void Damage(NetworkConnectionToClient source, NetworkConnectionToClient receiver, int totalDamage)
        {
            var playerData = _serverData.GetPlayerData(receiver);
            playerData.Health -= totalDamage;
            if (playerData.Health <= 0)
            {
                playerData.Health = 0;
                _serverData.AddKill(source, receiver);
            }
            else
            {
                receiver.identity.gameObject.GetComponent<HealthSystem>().health = playerData.Health;
            }
        }
    }
}
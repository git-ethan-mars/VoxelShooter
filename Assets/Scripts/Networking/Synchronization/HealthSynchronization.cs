using Infrastructure.Factory;
using Mirror;
using PlayerLogic;
using UnityEngine;

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
        public void Damage(NetworkConnectionToClient source, NetworkConnectionToClient receiver, int totalDamage)
        {
            var playerData = _serverData.GetPlayerData(receiver);
            var receiverPosition = receiver.identity.transform.position;
            var tombstonePosition = new Vector3(receiverPosition.x, receiverPosition.y, receiverPosition.z);
            if (!playerData.IsAlive) return;
            playerData.Health -= totalDamage;
            if (playerData.Health <= 0)
            {
                playerData.Health = 0;
                _serverData.AddKill(source, receiver);
                var tombstone = _entityFactory.CreateTombstone(tombstonePosition);
            }
            else
            {
                receiver.identity.gameObject.GetComponent<HealthSystem>().health = playerData.Health;
            }
        }
    }
}
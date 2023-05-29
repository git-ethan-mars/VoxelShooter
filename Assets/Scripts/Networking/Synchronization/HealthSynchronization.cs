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
            var result = _serverData.TryGetPlayerData(receiver, out var playerData);
            if (!result || !playerData.IsAlive) return;
            var receiverPosition = receiver.identity.transform.position;
            var tombstonePosition = new Vector3(receiverPosition.x, receiverPosition.y, receiverPosition.z);
            playerData.Health -= totalDamage;
            if (playerData.Health <= 0)
            {
                playerData.Health = 0;
                _serverData.AddKill(source, receiver);
                var tombstone = _entityFactory.CreateTombstone(tombstonePosition);
            }
            else
            {
                receiver.identity.gameObject.GetComponent<Player>().health = playerData.Health;
            }
        }
    }
}
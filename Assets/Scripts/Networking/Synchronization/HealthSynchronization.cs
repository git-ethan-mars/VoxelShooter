using Mirror;
using PlayerLogic;

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
            var result = _serverData.TryGetPlayerData(receiver, out var playerData);
            if (!result || !playerData.IsAlive) return;
            playerData.Health -= totalDamage;
            if (playerData.Health <= 0)
            {
                playerData.Health = 0;
                _serverData.AddKill(source, receiver);
            }
            else
            {
                receiver.identity.gameObject.GetComponent<Player>().health = playerData.Health;
            }
        }
    }
}
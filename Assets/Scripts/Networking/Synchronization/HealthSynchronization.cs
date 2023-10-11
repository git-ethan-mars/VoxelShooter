using Mirror;
using PlayerLogic;

namespace Networking.Synchronization
{
    public class HealthSynchronization : NetworkBehaviour
    {
        private IServer _server;

        public void Construct(IServer server)
        {
            _server = server;
        }


        [Server]
        public void Damage(NetworkConnectionToClient source, NetworkConnectionToClient receiver, int totalDamage)
        {
            var result = _server.ServerData.TryGetPlayerData(receiver, out var playerData);
            if (!result || !playerData.IsAlive) return;
            playerData.Health -= totalDamage;
            if (playerData.Health <= 0)
            {
                playerData.Health = 0;
                _server.AddKill(source, receiver);
            }
            else
            {
                receiver.identity.gameObject.GetComponent<Player>().health = playerData.Health;
            }
        }
    }
}
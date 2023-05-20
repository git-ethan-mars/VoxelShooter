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
        public void Damage(NetworkConnectionToClient source, NetworkConnectionToClient receiver, int totalDamage)
        {
            var playerData = _serverData.GetPlayerData(receiver);
            playerData.Health -= totalDamage;
            if (playerData.Health <= 0)
            {
                playerData.Health = 0;
                _serverData.AddKill(source, receiver);
                _playerFactory.CreateSpectatorPlayer(receiver);
                
            }
            else
            {
                receiver.identity.gameObject.GetComponent<HealthSystem>().health = playerData.Health;
            }
        }
    }
}
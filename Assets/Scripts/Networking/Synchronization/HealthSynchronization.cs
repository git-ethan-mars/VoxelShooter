using Mirror;
using Player;
using UnityEngine;

namespace Networking.Synchronization
{
    public class HealthSynchronization : NetworkBehaviour
    {
        [SerializeField] private HealthSystem healthSystem; 
        private ServerData _serverData;

        public void Construct(ServerData serverData)
        {
            _serverData = serverData;
        }
        
        
        [Command]
        public void CmdTakeDamage(int damage, NetworkConnectionToClient conn = null)
        {
            _serverData.GetPlayerData(conn.connectionId).Characteristic.health -= damage;
            if (_serverData.GetPlayerData(conn.connectionId).Characteristic.health <= 0)
            {
                Debug.Log("DIED");
            }
            SendHealth(_serverData.GetPlayerData(conn.connectionId).Characteristic.health);
        }

        [TargetRpc]
        private void SendHealth(int health)
        {
            healthSystem.Health = health;
        }
    }
}
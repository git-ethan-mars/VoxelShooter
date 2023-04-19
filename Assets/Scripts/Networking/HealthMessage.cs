using Mirror;

namespace Networking
{
    public struct HealthMessage : NetworkMessage
    {
        public int CurrentHealth;
        public int MaxHealth;
    }
}
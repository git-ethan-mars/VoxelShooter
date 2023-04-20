using Mirror;

namespace Networking.Messages
{
    public struct HealthMessage : NetworkMessage
    {
        public int CurrentHealth;
        public int MaxHealth;
    }
}
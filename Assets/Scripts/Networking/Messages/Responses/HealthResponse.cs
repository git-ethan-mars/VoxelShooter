using Mirror;

namespace Networking
{
    public struct HealthResponse : NetworkMessage
    {
        public readonly int Health;

        public HealthResponse(int health)
        {
            Health = health;
        }
    }
}
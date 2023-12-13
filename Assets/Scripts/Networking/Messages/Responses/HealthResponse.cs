using Mirror;

namespace Networking.Messages.Responses
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
using Data;
using Mirror;

namespace Networking.Messages
{
    public struct RespawnTimeMessage : NetworkMessage
    {
        public ServerTime TimeLeft;

        public RespawnTimeMessage(ServerTime timeLeft)
        {
            TimeLeft = timeLeft;
        }
    }
}
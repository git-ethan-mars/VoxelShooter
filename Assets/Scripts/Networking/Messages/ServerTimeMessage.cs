using Data;
using Mirror;

namespace Networking
{
    public struct ServerTimeMessage : NetworkMessage
    {
        public readonly ServerTime TimeLeft;
        public ServerTimeMessage(ServerTime timeLeft)
        {
            TimeLeft = timeLeft;
        }
    }
}
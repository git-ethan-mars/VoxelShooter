using Data;
using Mirror;

namespace Networking.Messages.Responses
{
    public struct ServerTimeResponse : NetworkMessage
    {
        public readonly ServerTime TimeLeft;
        public ServerTimeResponse(ServerTime timeLeft)
        {
            TimeLeft = timeLeft;
        }
    }
}
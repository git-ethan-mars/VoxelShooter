using Data;
using Mirror;

namespace Networking.Messages.Responses
{
    public struct RespawnTimeResponse : NetworkMessage
    {
        public ServerTime TimeLeft;

        public RespawnTimeResponse(ServerTime timeLeft)
        {
            TimeLeft = timeLeft;
        }
    }
}
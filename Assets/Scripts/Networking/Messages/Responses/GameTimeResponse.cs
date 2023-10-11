using Data;
using Mirror;

namespace Networking.Messages.Responses
{
    public struct GameTimeResponse : NetworkMessage
    {
        public readonly ServerTime TimeLeft;
        public GameTimeResponse(ServerTime timeLeft)
        {
            TimeLeft = timeLeft;
        }
    }
}
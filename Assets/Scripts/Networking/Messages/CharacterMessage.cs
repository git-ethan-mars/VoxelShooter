using Data;
using Mirror;

namespace Networking.Messages
{
    public struct ChangeClassRequest : NetworkMessage
    {
        public GameClass GameClass;

        public ChangeClassRequest(GameClass gameClass)
        {
            GameClass = gameClass;
        }
    }
}
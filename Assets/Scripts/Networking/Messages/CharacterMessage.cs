using Data;
using Mirror;

namespace Networking.Messages
{
    public struct ChangeClassRequest : NetworkMessage
    {
        public readonly GameClass GameClass;

        public ChangeClassRequest(GameClass gameClass)
        {
            GameClass = gameClass;
        }
    }
}
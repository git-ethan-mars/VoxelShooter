using Data;
using Mirror;

namespace Networking.Messages
{
    public struct ChangeClassRequest : NetworkMessage
    {
        public readonly GameClass GameClass;
        public readonly string NickName;

        public ChangeClassRequest(GameClass gameClass, string nickName)
        {
            GameClass = gameClass;
            NickName = nickName;
        }
    }
}
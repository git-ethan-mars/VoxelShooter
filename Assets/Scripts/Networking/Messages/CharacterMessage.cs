using Data;
using Mirror;

namespace Networking.Messages
{
    public struct CharacterMessage : NetworkMessage
    {
        public GameClass GameClass;
        public string NickName;
    }
}
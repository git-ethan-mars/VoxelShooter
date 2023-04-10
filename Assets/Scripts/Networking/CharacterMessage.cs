using Data;
using Mirror;

namespace Networking
{
    public struct CharacterMessage : NetworkMessage
    {
        public GameClass GameClass;
        public string NickName;
    }
}
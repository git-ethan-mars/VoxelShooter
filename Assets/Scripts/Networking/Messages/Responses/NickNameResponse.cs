using Mirror;

namespace Networking.Messages.Responses
{
    public struct NickNameResponse : NetworkMessage
    {
        public readonly NetworkIdentity Identity;
        public readonly string NickName;

        public NickNameResponse(NetworkIdentity identity, string nickName)
        {
            Identity = identity;
            NickName = nickName;
        }
    }
}
using Mirror;

namespace Networking.Messages.Responses
{
    public struct ChangeItemModelResponse : NetworkMessage
    {
        public readonly NetworkIdentity Identity;
        public readonly int ItemId;

        public ChangeItemModelResponse(NetworkIdentity identity, int itemId)
        {
            Identity = identity;
            ItemId = itemId;
        }
    }
}
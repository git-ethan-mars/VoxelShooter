using Mirror;

namespace Networking.Messages.Responses
{
    public struct ItemUseResponse : NetworkMessage
    {
        public readonly int ItemId;
        public readonly int Count;

        public ItemUseResponse(int itemId, int count)
        {
            ItemId = itemId;
            Count = count;
        }
    }
}
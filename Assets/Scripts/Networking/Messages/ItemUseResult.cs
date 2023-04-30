using Mirror;

namespace Networking.Messages
{
    public struct ItemUseResult : NetworkMessage
    {
        public readonly int ItemId;
        public readonly int Count;

        public ItemUseResult(int itemId, int count)
        {
            ItemId = itemId;
            Count = count;
        }
    }
}
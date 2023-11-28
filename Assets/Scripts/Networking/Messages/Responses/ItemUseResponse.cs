using Mirror;

namespace Networking.Messages.Responses
{
    public struct ItemUseResponse : NetworkMessage
    {
        public readonly int SlotIndex;
        public readonly int Count;

        public ItemUseResponse(int slotIndex, int count)
        {
            SlotIndex = slotIndex;
            Count = count;
        }
    }
}
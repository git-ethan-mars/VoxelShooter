using Mirror;

namespace Networking.Messages.Responses
{
    public struct RocketReloadResponse : NetworkMessage
    {
        public readonly int SlotIndex;
        public readonly int TotalRockets;
        public readonly int RocketsInSlotsCount;

        public RocketReloadResponse(int slotIndex, int totalRockets, int rocketsInSlotsCount)
        {
            SlotIndex = slotIndex;
            TotalRockets = totalRockets;
            RocketsInSlotsCount = rocketsInSlotsCount;
        }
    }
}
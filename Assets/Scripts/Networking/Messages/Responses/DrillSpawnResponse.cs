using Mirror;

namespace Networking.Messages.Responses
{
    public struct DrillSpawnResponse : NetworkMessage
    {
        public readonly int SlotIndex;
        public readonly int ChargedDrills;

        public DrillSpawnResponse(int slotIndex, int chargedDrills)
        {
            SlotIndex = slotIndex;
            ChargedDrills = chargedDrills;
        }
    }
}
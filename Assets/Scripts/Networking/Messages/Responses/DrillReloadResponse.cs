using Mirror;

namespace Networking.Messages.Responses
{
    public struct DrillReloadResponse : NetworkMessage
    {
        public readonly int SlotIndex;
        public readonly int ChargedDrills;
        public readonly int Amount;

        public DrillReloadResponse(int slotIndex, int chargedDrills, int amount)
        {
            SlotIndex = slotIndex;
            ChargedDrills = chargedDrills;
            Amount = amount;
        }
    }
}
using Mirror;

namespace Networking.Messages.Responses
{
    public struct RocketReloadResponse : NetworkMessage
    {
        public readonly int SlotIndex;
        public readonly int ChargedRockets;
        public readonly int CarriedRockets;

        public RocketReloadResponse(int slotIndex, int chargedRockets, int carriedRockets)
        {
            SlotIndex = slotIndex;
            ChargedRockets = chargedRockets;
            CarriedRockets = carriedRockets;
        }
    }
}
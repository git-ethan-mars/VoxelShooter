using Mirror;

namespace Networking.Messages.Responses
{
    public struct RocketSpawnResponse : NetworkMessage
    {
        public readonly int SlotIndex;
        public readonly int ChargedRockets;

        public RocketSpawnResponse(int slotIndex, int chargedRockets)
        {
            SlotIndex = slotIndex;
            ChargedRockets = chargedRockets;
        }
    }
}
using Mirror;

namespace Networking.Messages.Responses
{
    public struct ShootResultResponse : NetworkMessage
    {
        public readonly int SlotIndex;
        public readonly int BulletsInMagazine;
        public ShootResultResponse(int slotIndex, int bulletsInMagazine)
        {
            SlotIndex = slotIndex;
            BulletsInMagazine = bulletsInMagazine;
        }
    }
}
using Mirror;

namespace Networking.Messages.Responses
{
    public struct ReloadResultResponse : NetworkMessage
    {
        public readonly int SlotIndex;
        public readonly int TotalBullets;
        public readonly int BulletsInMagazine;

        public ReloadResultResponse(int slotIndex, int totalBullets, int bulletsInMagazine)
        {
            SlotIndex = slotIndex;
            TotalBullets = totalBullets;
            BulletsInMagazine = bulletsInMagazine;
        }
    }
}
using Mirror;

namespace Networking.Messages.Responses
{
    public struct ReloadResultResponse : NetworkMessage
    {
        public readonly int WeaponId;
        public readonly int TotalBullets;
        public readonly int BulletsInMagazine;

        public ReloadResultResponse(int weaponId, int totalBullets, int bulletsInMagazine)
        {
            WeaponId = weaponId;
            TotalBullets = totalBullets;
            BulletsInMagazine = bulletsInMagazine;
        }
    }
}
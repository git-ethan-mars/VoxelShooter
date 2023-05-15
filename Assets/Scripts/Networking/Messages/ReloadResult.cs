using Mirror;

namespace Inventory
{
    public struct ReloadResult : NetworkMessage
    {
        public readonly int WeaponId;
        public readonly int TotalBullets;
        public readonly int BulletsInMagazine;

        public ReloadResult(int weaponId, int totalBullets, int bulletsInMagazine)
        {
            WeaponId = weaponId;
            TotalBullets = totalBullets;
            BulletsInMagazine = bulletsInMagazine;
        }
    }
}
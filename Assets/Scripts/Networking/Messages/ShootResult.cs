using Mirror;

namespace Networking.Messages
{
    public struct ShootResult : NetworkMessage
    {
        public readonly int WeaponId;
        public readonly int BulletsInMagazine;
        public ShootResult(int weaponId, int bulletsInMagazine)
        {
            WeaponId = weaponId;
            BulletsInMagazine = bulletsInMagazine;
        }
    }
}
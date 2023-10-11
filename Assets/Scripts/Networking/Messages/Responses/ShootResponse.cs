using Mirror;

namespace Networking.Messages.Responses
{
    public struct ShootResponse : NetworkMessage
    {
        public readonly int WeaponId;
        public readonly int BulletsInMagazine;
        public ShootResponse(int weaponId, int bulletsInMagazine)
        {
            WeaponId = weaponId;
            BulletsInMagazine = bulletsInMagazine;
        }
    }
}
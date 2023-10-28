using Mirror;

namespace Networking.Messages.Responses
{
    public struct ShootResultResponse : NetworkMessage
    {
        public readonly int WeaponId;
        public readonly int BulletsInMagazine;
        public ShootResultResponse(int weaponId, int bulletsInMagazine)
        {
            WeaponId = weaponId;
            BulletsInMagazine = bulletsInMagazine;
        }
    }
}
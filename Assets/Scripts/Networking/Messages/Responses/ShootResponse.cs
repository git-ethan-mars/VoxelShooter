using Mirror;

namespace Networking.Messages.Responses
{
    public struct ShootResultResponse : NetworkMessage
    {
        public readonly int BulletsInMagazine;
        public ShootResultResponse(int bulletsInMagazine)
        {
            BulletsInMagazine = bulletsInMagazine;
        }
    }
}
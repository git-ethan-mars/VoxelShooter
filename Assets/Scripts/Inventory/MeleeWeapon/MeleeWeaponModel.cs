using Mirror;
using Networking.Messages.Requests;

namespace Inventory
{
    public class MeleeWeaponModel : IInventoryItemModel
    {
        private readonly RayCaster _rayCaster;

        public MeleeWeaponModel(RayCaster rayCaster)
        {
            _rayCaster = rayCaster;
        }

        public void WeakHit()
        {
            NetworkClient.Send(new HitRequest(_rayCaster.CentredRay, false));
        }

        public void StrongHit()
        {
            NetworkClient.Send(new HitRequest(_rayCaster.CentredRay, true));
        }
    }
}
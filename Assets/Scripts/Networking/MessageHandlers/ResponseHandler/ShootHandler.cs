using Inventory;
using Inventory.RangeWeapon;
using Networking.Messages.Responses;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class ShootResultHandler : ResponseHandler<ShootResultResponse>
    {
        private readonly InventorySystem _inventory;

        public ShootResultHandler(InventorySystem inventory)
        {
            _inventory = inventory;
        }

        protected override void OnResponseReceived(ShootResultResponse response)
        {
            ((RangeWeaponModel) _inventory.GetModel(response.SlotIndex)).BulletsInMagazine = response.BulletsInMagazine;
        }
    }
}
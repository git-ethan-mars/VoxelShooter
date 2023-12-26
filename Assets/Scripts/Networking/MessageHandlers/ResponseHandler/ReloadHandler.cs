using Inventory;
using Inventory.RangeWeapon;
using Networking.Messages.Responses;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class ReloadResultHandler : ResponseHandler<ReloadResultResponse>
    {
        private readonly InventorySystem _inventory;

        public ReloadResultHandler(InventorySystem inventory)
        {
            _inventory = inventory;
        }

        protected override void OnResponseReceived(ReloadResultResponse response)
        {
            var rangeWeapon = (RangeWeaponModel)_inventory.GetModel(response.SlotIndex);
            rangeWeapon.TotalBullets = response.TotalBullets;
            rangeWeapon.BulletsInMagazine = response.BulletsInMagazine;
        }
    }
}
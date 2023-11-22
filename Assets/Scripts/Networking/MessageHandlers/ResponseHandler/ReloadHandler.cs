using Inventory;
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
            var reloading = (IReloading) _inventory.GetModel(response.WeaponId);
            reloading.TotalBullets.Value = response.TotalBullets;
            reloading.BulletsInMagazine.Value = response.BulletsInMagazine;
        }
    }
}
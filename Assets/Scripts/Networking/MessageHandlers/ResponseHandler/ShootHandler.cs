using Inventory;
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
            ((IShooting) _inventory.ActiveItemModel).BulletsInMagazine.Value = response.BulletsInMagazine;
        }
    }
}
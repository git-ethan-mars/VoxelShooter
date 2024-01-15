using Inventory;
using Inventory.Drill;
using Networking.Messages.Responses;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class DrillReloadHandler : ResponseHandler<DrillReloadResponse>
    {
        private readonly InventorySystem _inventory;

        public DrillReloadHandler(InventorySystem inventory)
        {
            _inventory = inventory;
        }

        protected override void OnResponseReceived(DrillReloadResponse response)
        {
            var drill = (DrillModel) _inventory.GetModel(response.SlotIndex);
            drill.CarriedDrills = response.Amount;
            drill.ChargedDrills = response.ChargedDrills;
        }
    }
}
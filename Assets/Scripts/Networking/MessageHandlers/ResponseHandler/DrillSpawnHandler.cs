using Inventory;
using Inventory.Drill;
using Networking.Messages.Responses;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class DrillSpawnHandler : ResponseHandler<DrillSpawnResponse>
    {
        private readonly InventorySystem _inventory;

        public DrillSpawnHandler(InventorySystem inventory)
        {
            _inventory = inventory;
        }

        protected override void OnResponseReceived(DrillSpawnResponse response)
        {
            ((DrillModel) _inventory.GetModel(response.SlotIndex)).ChargedDrills = response.ChargedDrills;
        }
    }
}
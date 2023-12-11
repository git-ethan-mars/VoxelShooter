using Inventory;
using Inventory.RocketLauncher;
using Networking.Messages.Responses;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class RocketReloadHandler : ResponseHandler<RocketReloadResponse>
    {
        private readonly InventorySystem _inventory;

        public RocketReloadHandler(InventorySystem inventory)
        {
            _inventory = inventory;
        }

        protected override void OnResponseReceived(RocketReloadResponse response)
        {
            var reloading = (IRocketReloadable) _inventory.GetModel(response.SlotIndex);
            reloading.Count.Value = response.TotalRockets;
            reloading.RocketsInSlotsCount.Value = response.RocketsInSlotsCount;
        }
    }
}
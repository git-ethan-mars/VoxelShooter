using Inventory;
using Inventory.RocketLauncher;
using Networking.Messages.Responses;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class RocketSpawnHandler : ResponseHandler<RocketSpawnResponse>
    {
        private readonly InventorySystem _inventory;

        public RocketSpawnHandler(InventorySystem inventory)
        {
            _inventory = inventory;
        }

        protected override void OnResponseReceived(RocketSpawnResponse response)
        {
            ((RocketLauncherModel) _inventory.GetModel(response.SlotIndex)).ChargedRockets = response.ChargedRockets;
        }
    }
}
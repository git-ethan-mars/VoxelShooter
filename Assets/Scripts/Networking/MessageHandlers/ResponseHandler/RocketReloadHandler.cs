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
            var rocketLauncher = (RocketLauncherModel) _inventory.GetModel(response.SlotIndex);
            rocketLauncher.CarriedRockets = response.CarriedRockets;
            rocketLauncher.ChargedRockets = response.ChargedRockets;
        }
    }
}
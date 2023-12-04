using Inventory;
using Networking.Messages.Responses;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class ChangeSlotResultHandler : ResponseHandler<ChangeSlotResponse>
    {
        private readonly InventorySystem _inventory;

        public ChangeSlotResultHandler(InventorySystem inventory)
        {
            _inventory = inventory;
        }

        protected override void OnResponseReceived(ChangeSlotResponse response)
        {
            _inventory.SwitchActiveSlot(response.Index);
        }
    }
}
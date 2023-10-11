using Inventory;
using Networking.Messages.Responses;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class ChangeSlotHandler : ResponseHandler<ChangeSlotResponse>
    {
        private readonly InventoryController _inventoryController;

        public ChangeSlotHandler(InventoryController inventoryController)
        {
            _inventoryController = inventoryController;
        }
        protected override void OnResponseReceived(ChangeSlotResponse response)
        {
            _inventoryController.Boarders[_inventoryController.ItemIndex].SetActive(false);
            _inventoryController.Slots[_inventoryController.ItemIndex].ItemHandler.Unselect();
            _inventoryController.ItemIndex = response.Index;
            _inventoryController.Slots[_inventoryController.ItemIndex].ItemHandler.Select();
            _inventoryController.Boarders[_inventoryController.ItemIndex].SetActive(true);

        }
    }
}
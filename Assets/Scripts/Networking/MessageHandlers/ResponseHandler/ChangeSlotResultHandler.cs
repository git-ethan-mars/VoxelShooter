using Inventory;
using Networking.Messages.Responses;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class ChangeSlotResultHandler : ResponseHandler<ChangeSlotResultResponse>
    {
        private readonly InventoryController _inventoryController;

        public ChangeSlotResultHandler(InventoryController inventoryController)
        {
            _inventoryController = inventoryController;
        }

        protected override void OnResponseReceived(ChangeSlotResultResponse response)
        {
            _inventoryController.Boarders[_inventoryController.ItemIndex].SetActive(false);
            _inventoryController.Slots[_inventoryController.ItemIndex].ItemHandler.Unselect();
            _inventoryController.ItemIndex = response.Index;
            _inventoryController.Slots[_inventoryController.ItemIndex].ItemHandler.Select();
            _inventoryController.Boarders[_inventoryController.ItemIndex].SetActive(true);
        }
    }
}
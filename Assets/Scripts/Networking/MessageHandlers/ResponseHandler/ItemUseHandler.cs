using Inventory;
using Networking.Messages.Responses;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class ItemUseHandler : ResponseHandler<ItemUseResponse>
    {
        private readonly InventorySystem _inventory;

        public ItemUseHandler(InventorySystem inventory)
        {
            _inventory = inventory;
        }

        protected override void OnResponseReceived(ItemUseResponse response)
        {
            ((IConsumable) _inventory.GetModel(response.SlotIndex)).Count.Value = response.Count;
        }
    }
}
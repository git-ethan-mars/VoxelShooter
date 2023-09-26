using System.Collections.Generic;
using Inventory;
using Networking.Messages.Responses;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class ItemUseHandler : ResponseHandler<ItemUseResponse>
    {
        private readonly List<Slot> _slots;

        public ItemUseHandler(List<Slot> slots)
        {
            _slots = slots;
        }

        protected override void OnResponseReceived(ItemUseResponse response)
        {
            ((IConsumable) _slots.Find(slot => slot.InventoryItem.id == response.ItemId).ItemHandler).Count =
                response.Count;
            ((IConsumable) _slots.Find(slot => slot.InventoryItem.id == response.ItemId).ItemHandler).OnCountChanged();
        }
    }
}
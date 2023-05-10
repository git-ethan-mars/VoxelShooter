using Data;

namespace Inventory
{
    public class Slot
    {
        public readonly InventoryItem InventoryItem;
        public readonly IInventoryItemView ItemHandler;

        public Slot(InventoryItem inventoryItem, IInventoryItemView itemHandler)
        {
            InventoryItem = inventoryItem;
            ItemHandler = itemHandler;
        }
        
    }
}
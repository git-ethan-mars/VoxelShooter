using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "Block", menuName = "Inventory System/Inventory Items/Block")]
    public class BlockItem : InventoryItem
    {
        public void Awake()
        {
            itemType = ItemType.Block;
        }
    }
}
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "Brush", menuName = "Inventory System/Inventory Items/Brush  ")]
    public class BrushItem : InventoryItem
    {
        public void Awake()
        {
            itemType = ItemType.Brush;
        }
    }
}
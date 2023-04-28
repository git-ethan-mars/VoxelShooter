using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "Tnt", menuName = "Inventory System/Inventory Items/Tnt")]
    public class TntItem : InventoryItem
    {
        public int count;
        public float delayInSeconds;
        [Range(0, int.MaxValue)]
        public int radius;

        public void Awake()
        {
            itemType = ItemType.Tnt;
        }
    }
}
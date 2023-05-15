using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "Tnt", menuName = "Inventory System/Inventory Items/Tnt")]
    public class TntItem : InventoryItem
    {
        public Sprite countIcon;
        public int count;
        public float delayInSeconds;
        public int radius;

        public void Awake()
        {
            itemType = ItemType.Tnt;
        }
    }
}
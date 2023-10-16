using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "Drill", menuName = "Inventory System/Inventory Items/Drill")]
    public class DrillItem : InventoryItem
    {
        public Sprite countIcon;
        public int count;
        public int radius;
        public int damage;
        public int speed;

        public void Awake()
        {
            itemType = ItemType.Drill;
        }
    }
}
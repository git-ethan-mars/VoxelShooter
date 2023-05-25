using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "RocketLauncher", menuName = "Inventory System/Inventory Items/RocketLauncher")]
    public class RocketLauncherItem : InventoryItem
    {
        public Sprite countIcon;
        public int count;
        public int radius;
        public int damage;
        public int speed;
        public int particlesSpeed;
        public int particlesCount;

        public void Awake()
        {
            itemType = ItemType.RocketLauncher;
        }
    }
}
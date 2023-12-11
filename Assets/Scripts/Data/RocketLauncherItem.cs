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
        public float reloadTime;
        public int rocketSlotsCount;
        public int rechargeableRocketsCount;
        public int particlesSpeed;
        public int particlesCount;
        public AudioData explosionSound;
        public AudioData reloadSound;

        public void Awake()
        {
            itemType = ItemType.RocketLauncher;
        }
    }
}
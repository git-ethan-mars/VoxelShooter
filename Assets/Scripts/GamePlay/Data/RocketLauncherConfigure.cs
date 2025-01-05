using UnityEngine;
using VoxelMap;

namespace GamePlay.Data
{
    [CreateAssetMenu(fileName = "RocketLauncher", menuName = "Inventory System/Inventory Items/RocketLauncher")]
    public class RocketLauncherConfigure : ItemConfigure
    {
        public Sprite countIcon;
        public int count;
        public int radius;
        public int damage;
        public int speed;
        public float reloadTime;
        public int chargedRocketsCapacity;
        public int rechargeableRocketsCount;
        public int particlesSpeed;
        public int particlesCount;
        public AudioData explosionSound;
        public AudioData reloadSound;

        public override InventoryItem CreateModel(IInventoryFactory inventoryFactory, RayCaster rayCaster, MapProvider mapProvider)
        {
            return inventoryFactory.CreateRocketLauncher(new RocketLauncherData(this), rayCaster, mapProvider);
        }
    }
}
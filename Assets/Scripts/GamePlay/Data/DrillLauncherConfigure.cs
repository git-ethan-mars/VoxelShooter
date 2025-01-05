using Common;
using UnityEngine;
using VoxelMap;

namespace GamePlay.Data
{
    [CreateAssetMenu(fileName = "Drill", menuName = "Inventory System/Inventory Items/Drill")]
    public class DrillLauncherConfigure : ItemConfigure
    {
        public Sprite countIcon;
        public int count;
        public int radius;
        public int damage;
        public int speed;
        public int chargedDrillsCapacity;
        public int lifetime;
        public int rotationSpeed;
        public float reloadTime;
        public AudioData impactSound;
        public AudioData reloadSound;

        public override InventoryItem CreateModel(IInventoryFactory inventoryFactory, RayCaster rayCaster, MapProvider mapProvider)
        {
            return inventoryFactory.CreateDrillLauncher(new DrillLauncherData(this), rayCaster);
        }
    }
}
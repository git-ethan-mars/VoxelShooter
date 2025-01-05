using Common;
using UnityEngine;
using UnityEngine.Serialization;
using VoxelMap;

namespace GamePlay.Data
{
    [CreateAssetMenu(fileName = "Melee Weapon", menuName = "Inventory System/Inventory Items/Melee weapon")]
    public class MeleeWeaponConfigure : ItemConfigure
    {
        public MeleeWeaponType type;
        
        [Header("Configuration")] 
        public float range;
        public float timeBetweenHit;
        
        [Header("Damage")]
        public int damageToPlayer;
        [Range(0,5)]
        public float headMultiplier;
        [Range(0,5)]
        public float chestMultiplier;
        [Range(0,5)]
        public float legMultiplier;
        [Range(0,5)]
        public float armMultiplier;
        [FormerlySerializedAs("damageToBlock")] 
        public int damageToVoxel;
        public bool hasStrongHit;
        
        [Header("Audio")]
        public AudioData diggingAudio;
        public AudioData hittingAudio;
        
        public override InventoryItem CreateModel(IInventoryFactory inventoryFactory, RayCaster rayCaster, MapProvider mapProvider)
        {
            return inventoryFactory.CreateMeleeWeapon(new MeleeWeaponData(this), rayCaster);
        }
    }
}
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "Range Weapon", menuName = "Inventory System/Inventory Items/Weapon")]
    public class RangeWeaponItem : InventoryItem
    {
        public Sprite ammoIcon;
        [Header("Configuration")] 
        public bool isAutomatic;
        public float timeBetweenShooting;
        public float baseRecoil;
        public float stepRecoil;
        public float resetTimeRecoil;
        public float range;
        public float reloadTime;
        public float timeBetweenShots;
        public int magazineSize;
        public int bulletsPerTap;
        public int totalBullets;
        [Header("Damage")]
        public int damage;
        [Range(0,5)]
        public float headMultiplier;
        [Range(0,5)]
        public float chestMultiplier;
        [Range(0,5)]
        public float legMultiplier;
        [Range(0,5)]
        public float armMultiplier;
        
        [Header("Audio")]
        public AudioClip shootingAudioClip;
        [Range(0, 1)] public float shootingVolume;
        public AudioClip reloadingAudioClip;
        [Range(0, 1)] public float reloadingVolume;

        private void Awake()
        {
            itemType = ItemType.RangeWeapon;
        }
    }
}
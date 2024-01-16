using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "Range Weapon", menuName = "Inventory System/Inventory Items/Weapon")]
    public class RangeWeaponItem : InventoryItem
    {
        public Sprite ammoIcon;
        public Sprite scopeIcon;

        [Header("Configuration")]
        public bool isAutomatic;

        public float timeBetweenShooting;
        public float baseRecoil;
        public float stepRecoil;
        public float resetTimeRecoil;
        public float range;
        public float reloadTime;
        public int magazineSize;
        public int bulletsPerTap;
        public int totalBullets;
        public float zoomMultiplier;

        [Header("Damage")]
        public int damage;

        [Range(0, 5)]
        public float headMultiplier;

        [Range(0, 5)]
        public float chestMultiplier;

        [Range(0, 5)]
        public float legMultiplier;

        [Range(0, 5)]
        public float armMultiplier;

        [Header("Audio")]
        public AudioData shootingSound;

        public AudioData reloadingSound;

        private void Awake()
        {
            itemType = ItemType.RangeWeapon;
        }
    }
}
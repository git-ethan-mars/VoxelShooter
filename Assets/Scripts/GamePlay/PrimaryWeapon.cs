using UnityEngine;

namespace GamePlay
{
    [CreateAssetMenu(fileName = "Primary Weapon", menuName = "Inventory System/Items/Primary weapon")]
    public class PrimaryWeapon : InventoryItem
    {
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

        private void Awake()
        {
            itemType = ItemType.PrimaryWeapon;
        }
    }
}
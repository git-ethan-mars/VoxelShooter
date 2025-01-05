using System;
using Common;
using Common.AssetManagement;
using GamePlay.Entities;
using UnityEngine;
using VoxelMap;

namespace GamePlay.Data
{
    [CreateAssetMenu(fileName = "Range Weapon", menuName = "Inventory System/Inventory Items/Weapon")]
    public class RangeWeaponConfigure : ItemConfigure
    {
        public RangeWeaponType type;
        
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

        public override InventoryItem CreateModel(IInventoryFactory inventoryFactory, RayCaster rayCaster, MapProvider mapProvider)
        {
            return inventoryFactory.CreateRangeWeapon(new RangeWeaponData(this), rayCaster);
        }
    }
}
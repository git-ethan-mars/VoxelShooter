using UnityEngine;

namespace Data
{
    public class RangeWeaponData
    {
        public readonly bool IsAutomatic;
        public readonly float TimeBetweenShooting;
        public readonly float BaseRecoil;
        public readonly float StepRecoil;
        public readonly float Range;
        public readonly float ReloadTime;
        public readonly int MagazineSize;
        public int TotalBullets;
        public float RecoilModifier;
        public readonly float ResetTimeRecoil;
        public int BulletsInMagazine;
        public bool IsReady;
        public readonly GameObject Prefab;
        public readonly int BulletsPerShot;
        public readonly int Damage;
        public readonly float ZoomMultiplier;
        public readonly Sprite InventoryIcon;
        public readonly int ID;
        public bool IsReloading;
        public readonly float HeadMultiplier;
        public readonly float ChestMultiplier;
        public readonly float LegMultiplier;
        public readonly float ArmMultiplier;
        public readonly AudioClip ReloadingAudioClip;
        public readonly AudioClip ShootAudioClip;
        public readonly Sprite AmmoTypeIcon;
        public readonly float ReloadingVolume;
        public readonly float ShootingVolume;


        public RangeWeaponData(RangeWeaponItem primaryRangeWeapon)
        {
            Damage = primaryRangeWeapon.damage;
            HeadMultiplier = primaryRangeWeapon.headMultiplier;
            ChestMultiplier = primaryRangeWeapon.chestMultiplier;
            LegMultiplier = primaryRangeWeapon.legMultiplier;
            ArmMultiplier = primaryRangeWeapon.armMultiplier;
            ReloadTime = primaryRangeWeapon.reloadTime;
            BaseRecoil = primaryRangeWeapon.baseRecoil;
            StepRecoil = primaryRangeWeapon.stepRecoil;
            BulletsInMagazine = primaryRangeWeapon.magazineSize;
            MagazineSize = primaryRangeWeapon.magazineSize;
            IsReady = true;
            TotalBullets = primaryRangeWeapon.totalBullets;
            BulletsPerShot = primaryRangeWeapon.bulletsPerTap;
            TimeBetweenShooting = primaryRangeWeapon.timeBetweenShooting;
            ResetTimeRecoil = primaryRangeWeapon.resetTimeRecoil;
            IsAutomatic = primaryRangeWeapon.isAutomatic;
            Damage = primaryRangeWeapon.damage;
            Prefab = primaryRangeWeapon.prefab;
            InventoryIcon = primaryRangeWeapon.inventoryIcon;
            AmmoTypeIcon = primaryRangeWeapon.ammoIcon;
            ID = primaryRangeWeapon.id;
            Range = primaryRangeWeapon.range;
            ReloadingAudioClip = primaryRangeWeapon.reloadingAudioClip;
            ShootAudioClip = primaryRangeWeapon.shootingAudioClip;
            ReloadingVolume = primaryRangeWeapon.reloadingVolume;
            ShootingVolume = primaryRangeWeapon.shootingVolume;
            ZoomMultiplier = primaryRangeWeapon.zoomMultiplier;
        }
    }
}
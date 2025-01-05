using System;
using UnityEngine;

namespace GamePlay.Data
{
	public class RangeWeaponData
	{
		public int ID => _rangeWeapon.id;
		public Sprite InventoryIcon => _rangeWeapon.inventoryIcon;
		public RangeWeaponType Type => _rangeWeapon.type;
		public int Damage => _rangeWeapon.damage;
		public float HeadMultiplier => _rangeWeapon.headMultiplier;
		public float LegMultiplier => _rangeWeapon.legMultiplier;

		public float ChestMultiplier => _rangeWeapon.chestMultiplier;
		public float ArmMultiplier => _rangeWeapon.armMultiplier;
		public float Range => _rangeWeapon.range;
		public bool IsAutomatic => _rangeWeapon.isAutomatic;

		public int BulletsPerTap => _rangeWeapon.bulletsPerTap;
		public float StepRecoil => _rangeWeapon.stepRecoil;
		public int MagazineSize => _rangeWeapon.magazineSize;
		public float BaseRecoil => _rangeWeapon.baseRecoil;
		public float ReloadTime => _rangeWeapon.reloadTime;
		public float ResetTimeRecoil => _rangeWeapon.resetTimeRecoil;
		public float TimeBetweenShooting => _rangeWeapon.timeBetweenShooting;
		public AudioData ShootingSound => _rangeWeapon.shootingSound;
		public AudioData ReloadingSound => _rangeWeapon.reloadingSound;
		public Sprite AmmoIcon => _rangeWeapon.ammoIcon;
		public Sprite ScopeIcon => _rangeWeapon.scopeIcon;

		public event Action<int> TotalBulletsChanged;
		public int TotalBullets
		{
			get => _totalBullets;
			set
			{
				_totalBullets = value;
				TotalBulletsChanged?.Invoke(value);
			}
		}

		private int _totalBullets;

		public float RecoilModifier { get; set; }
		
		public event Action<int> BulletsInMagazineChanged;

		public int BulletsInMagazine
		{
			get => _bulletsInMagazine;
			set
			{
				_bulletsInMagazine = value;
				BulletsInMagazineChanged?.Invoke(value);
			}
		}

		private int _bulletsInMagazine;
		
		public bool IsReady { get; set; } = true;
		public bool IsReloading { get; set; }

		private readonly RangeWeaponConfigure _rangeWeapon;
		
		public RangeWeaponData(RangeWeaponConfigure rangeWeapon)
		{
			_rangeWeapon = rangeWeapon;
			_totalBullets = rangeWeapon.totalBullets;
			_bulletsInMagazine = rangeWeapon.magazineSize;
		}
	}
}
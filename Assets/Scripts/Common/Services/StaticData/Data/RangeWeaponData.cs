namespace Common.StaticData
{
    public class RangeWeaponData : IItemData
    {
        public int ID => _rangeWeapon.id;
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
        public int TotalBullets { get; set; }
        public float RecoilModifier { get; set; }
        public int BulletsInMagazine { get; set; }
        public bool IsReady { get; set; }
        public bool IsReloading { get; set; }

        private readonly RangeWeaponItem _rangeWeapon;



        public RangeWeaponData(RangeWeaponItem rangeWeapon)
        {
            _rangeWeapon = rangeWeapon;
            TotalBullets = rangeWeapon.totalBullets;
            RecoilModifier = 0.0f;
            BulletsInMagazine = rangeWeapon.magazineSize;
            IsReady = true;
            IsReloading = false;
        }
    }
}
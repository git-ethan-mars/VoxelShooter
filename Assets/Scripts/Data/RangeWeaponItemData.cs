namespace Data
{
    public class RangeWeaponItemData : IMutableItemData
    {
        public int TotalBullets { get; set; }
        public float RecoilModifier { get; set; }
        public int BulletsInMagazine { get; set; }
        public bool IsReady { get; set; }
        public bool IsReloading { get; set; }

        public RangeWeaponItemData(RangeWeaponItem rangeWeapon)
        {
            TotalBullets = rangeWeapon.totalBullets;
            RecoilModifier = 0.0f;
            BulletsInMagazine = rangeWeapon.magazineSize;
            IsReady = true;
            IsReloading = false;
        }
    }
}
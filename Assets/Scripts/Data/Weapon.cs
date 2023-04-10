using System;

namespace Data
{
    public class Weapon
    {
        public float RecoilModifier;
        public readonly string Guid;
        public readonly float StepRecoil;
        public int BulletsInMagazine;
        public readonly int MagazineSize;
        public bool _readyToShoot;
        public int TotalBullets;
        public int BulletsPerShot;
        
        public float timeBetweenShooting;
        public float timeBetweenShots;
        public float resetTimeRecoil;

        public Weapon(PrimaryWeapon primaryWeapon)
        {
            Guid = System.Guid.NewGuid().ToString();
            StepRecoil = primaryWeapon.stepRecoil;
            BulletsInMagazine = primaryWeapon.magazineSize;
            MagazineSize = primaryWeapon.magazineSize;
            _readyToShoot = true;
            TotalBullets = primaryWeapon.totalBullets;
            BulletsPerShot = primaryWeapon.bulletsPerTap;
            timeBetweenShooting = primaryWeapon.timeBetweenShooting;
            timeBetweenShots = primaryWeapon.timeBetweenShots;
            resetTimeRecoil = primaryWeapon.resetTimeRecoil;
        }
    }
}
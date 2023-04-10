using System.Linq;
using Inventory;
using Mirror;
using Player;
using UnityEngine;

namespace Networking.Synchronization
{
    public class StatSynchronization : NetworkBehaviour
    {
        private HealthSystem _healthSystem;
        private GunSystem[] _gunSystems;
        
        public void Construct(HealthSystem healthSystem, GunSystem[] gunSystems)
        {
            _healthSystem = healthSystem;
            _gunSystems = gunSystems;
        }
        
        [Command]
        public void TakeDamage(int damage)
        {
            _healthSystem.Health -= damage;
            if (_healthSystem.Health <= 0)
            {
                Debug.Log(gameObject.name);
            }
            SendHealth(_healthSystem.Health);
        }

        [TargetRpc]
        private void SendHealth(int health)
        {
            _healthSystem.Health -= health;
        }

        [Command]
        public void OnShoot(string guid)
        {
            var gunSystem = _gunSystems.First(x => x.Guid == guid);
            if (gunSystem._readyToShoot)
            {
                gunSystem.BulletsInMagazine -= gunSystem.BulletsPerShot;
                if (gunSystem.BulletsInMagazine <= 0)
                    gunSystem.BulletsInMagazine = 0;
                gunSystem._readyToShoot = false;
                gunSystem.RecoilModifier += gunSystem.StepRecoil;
                SendBulletsCount(gunSystem.BulletsInMagazine, guid);
                gunSystem.StartShootCoroutines();
            }
        }

        [Command]
        public void OnReload(string guid)
        {
            var gunSystem = _gunSystems.First(x => x.Guid == guid);
            gunSystem.BulletsInMagazine = gunSystem.MagazineSize;
            gunSystem.TotalBullets -= gunSystem.MagazineSize;
            if (gunSystem.TotalBullets <= 0)
                gunSystem.TotalBullets = 0;
            SendReload(gunSystem.TotalBullets, guid);
        }
        
        [TargetRpc]
        private void SendBulletsCount(int bulletsInMagazine, string guid)
        {
            var gunSystem = _gunSystems.First(x => x.Guid == guid);
            gunSystem.BulletsInMagazine = bulletsInMagazine;
        }

        [TargetRpc]
        private void SendReload(int totalBullets, string guid)
        {
            var gunSystem = _gunSystems.First(x => x.Guid == guid);
            gunSystem.TotalBullets = totalBullets;
            gunSystem.StartReloadCoroutine();
        }
        
        
    }
}
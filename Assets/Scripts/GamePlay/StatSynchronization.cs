using System.Linq;
using Mirror;
using UnityEngine;

namespace GamePlay
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
            var gunSystem = _gunSystems.First(x => x.guid == guid);
            if (gunSystem._readyToShoot)
            {
                gunSystem.bulletsInMagazine -= gunSystem._bulletsPerShot;
                if (gunSystem.bulletsInMagazine <= 0)
                    gunSystem.bulletsInMagazine = 0;
                gunSystem._readyToShoot = false;
                gunSystem._recoilModifier += gunSystem.stepRecoil;
                SendBulletsCount(gunSystem.bulletsInMagazine, guid);
                gunSystem.StartShootCoroutines();
            }
        }

        [Command]
        public void OnReload(string guid)
        {
            var gunSystem = _gunSystems.First(x => x.guid == guid);
            gunSystem.bulletsInMagazine = gunSystem.magazineSize;
            gunSystem.totalBullets -= gunSystem.magazineSize;
            if (gunSystem.totalBullets <= 0)
                gunSystem.totalBullets = 0;
            SendReload(gunSystem.totalBullets, guid);
        }
        
        [TargetRpc]
        private void SendBulletsCount(int bulletsInMagazine, string guid)
        {
            var gunSystem = _gunSystems.First(x => x.guid == guid);
            gunSystem.bulletsInMagazine = bulletsInMagazine;
        }

        [TargetRpc]
        private void SendReload(int totalBullets, string guid)
        {
            var gunSystem = _gunSystems.First(x => x.guid == guid);
            gunSystem.totalBullets = totalBullets;
            gunSystem.StartReloadCoroutine();
        }
        
        
    }
}
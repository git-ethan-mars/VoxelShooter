using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using Infrastructure;
using Inventory;
using Mirror;
using Player;
using UnityEngine;

namespace Networking.Synchronization
{
    public class StatSynchronization : NetworkBehaviour
    {
        private HealthSystem _healthSystem;
        private List<Weapon> _weapons = new List<Weapon>();

        public void Construct(HealthSystem healthSystem, List<PrimaryWeapon> primaryWeapons)
        {
            _healthSystem = healthSystem;
            foreach (var primaryWeapon in primaryWeapons)
            {
                _weapons.Add(new Weapon(primaryWeapon));
            }
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
            var weapon = _weapons.First();
            if (weapon._readyToShoot)
            {
                weapon.BulletsInMagazine -= weapon.BulletsPerShot;
                if (weapon.BulletsInMagazine <= 0)
                    weapon.BulletsInMagazine = 0;
                weapon._readyToShoot = false;
                weapon.RecoilModifier += weapon.StepRecoil;
                SendBulletsCount(weapon.BulletsInMagazine, guid);
                StartShootCoroutines(weapon);
            }
        }

        [Command]
        public void OnReload(string guid)
        {
            var weapon = _weapons.First();
            weapon.BulletsInMagazine = weapon.MagazineSize;
            weapon.TotalBullets -= weapon.MagazineSize;
            if (weapon.TotalBullets <= 0)
                weapon.TotalBullets = 0;
            SendReload(weapon.TotalBullets, guid);
            //StartReloadCoroutine(weapon);
        }
        
        [TargetRpc]
        private void SendBulletsCount(int bulletsInMagazine, string guid)
        {
            var weapon = _weapons.First();
            weapon.BulletsInMagazine = bulletsInMagazine;
        }

        [TargetRpc]
        private void SendReload(int totalBullets, string guid)
        {
            var weapon = _weapons.First();
            weapon.TotalBullets = totalBullets;
        }
        
        public void StartShootCoroutines(Weapon weapon)
        {
            StartCoroutine(WaitForSeconds(() => ResetShoot(weapon), weapon.timeBetweenShooting));
            StartCoroutine(WaitForSeconds(() => ResetRecoil(weapon), weapon.resetTimeRecoil)); 
            //if (BulletsPerShot > 0 && BulletsInMagazine > 0)
            //    _coroutineRunner.StartCoroutine(WaitForSeconds(Shoot, weapon.timeBetweenShots));
        }
        
        private void ResetRecoil(Weapon weapon)
        {
            weapon.RecoilModifier -= weapon.StepRecoil;
        }

        private void ResetShoot(Weapon weapon)
        {
            weapon._readyToShoot = true;
        }
        
        private static IEnumerator WaitForSeconds(Action action, float timeInSeconds)
        {
            yield return new WaitForSeconds(timeInSeconds);
            action();
        }
    }
}
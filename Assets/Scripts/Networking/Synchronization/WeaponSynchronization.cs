using System;
using System.Collections;
using Data;
using Infrastructure.Factory;
using Mirror;
using UnityEngine;

namespace Networking.Synchronization
{
    public class WeaponSynchronization : NetworkBehaviour
    {
        private ServerData _serverData;
        private IParticleFactory _particleFactory;
        private AudioSource _audioSource;

        public void Construct(IParticleFactory particleFactory, ServerData serverData, AudioSource audioSource)
        {
            _serverData = serverData;
            _audioSource = audioSource;
            _particleFactory = particleFactory;
        }

        [Command]
        public void CmdShootSingle(Ray ray, int weaponId, NetworkConnectionToClient conn = null)
        {
            var weapon = _serverData.GetPlayerData(conn!.connectionId).WeaponsById[weaponId];
            if (!CanShoot(weapon) || weapon.IsAutomatic) return;
            ApplyRaycast(ray, weapon);
            Shoot(weapon);
        }

        [Command]
        public void CmdShootAutomatic(Ray ray, int weaponId, NetworkConnectionToClient conn = null)
        {
            var weapon = _serverData.GetPlayerData(conn!.connectionId).WeaponsById[weaponId];
            if (!CanShoot(weapon) || !weapon.IsAutomatic) return;
            ApplyRaycast(ray, weapon);
            Shoot(weapon);
        }

        [Command]
        public void CmdReload(int weaponId, NetworkConnectionToClient conn = null)
        {
            var weapon = _serverData.GetPlayerData(conn!.connectionId).WeaponsById[weaponId];
            if (CanReload(weapon))
            {
                Reload(weapon);
            }
        }


        [TargetRpc]
        private void SendWeaponState(int weaponId, int bulletsInMagazine)
        {
            var weapon = GetComponent<PlayerLogic.Inventory>().Weapons[weaponId];
            var audioSource = GetComponent<AudioSource>();
            audioSource.clip = weapon.ShootingAudioClip;
            audioSource.volume = weapon.ShootingVolume;
            audioSource.Play();
            weapon.BulletsInMagazine = bulletsInMagazine;
        }

        [TargetRpc]
        private void SendReload(int weaponId, int totalBullets, int bulletsInMagazine)
        {
            var weapon = GetComponent<PlayerLogic.Inventory>().Weapons[weaponId];
            var audioSource = GetComponent<AudioSource>();
            audioSource.clip = weapon.ReloadingAudioClip;
            audioSource.volume = weapon.ReloadingVolume;
            audioSource.Play();
            weapon.TotalBullets = totalBullets;
            weapon.BulletsInMagazine = bulletsInMagazine;
        }

        [Server]
        private void StartShootCoroutines(Weapon weapon)
        {
            StartCoroutine(WaitForSeconds(() => ResetShoot(weapon), weapon.TimeBetweenShooting));
            StartCoroutine(WaitForSeconds(() => ResetRecoil(weapon), weapon.ResetTimeRecoil));
            /*if (weapon.BulletsPerShot > 0 && weapon.BulletsInMagazine > 0)
            {
                Shoot(weapon);
                weapon.BulletsPerShot--;
            }*/
        }

        [Server]
        private void StartReloadCoroutine(Weapon weapon)
        {
            StartCoroutine(WaitForSeconds(() => ReloadFinished(weapon), weapon.ReloadTime));
        }

        [Server]
        private void ResetRecoil(Weapon weapon)
        {
            weapon.RecoilModifier -= weapon.StepRecoil;
        }

        [Server]
        private void ResetShoot(Weapon weapon)
        {
            weapon.IsReady = true;
        }

        [Server]
        private void Reload(Weapon weapon)
        {
            _audioSource.clip = weapon.ReloadingAudioClip;
            _audioSource.volume = weapon.ReloadingVolume;
            _audioSource.Play();
            
            weapon.IsReloading = true;
            if (weapon.TotalBullets + weapon.BulletsInMagazine - weapon.MagazineSize <= 0)
            {
                weapon.BulletsInMagazine += weapon.TotalBullets;
                weapon.TotalBullets = 0;
            }
            else
            {
                weapon.TotalBullets -= weapon.MagazineSize - weapon.BulletsInMagazine;
                weapon.BulletsInMagazine = weapon.MagazineSize;
            }
            SendReload(weapon.ID, weapon.TotalBullets, weapon.BulletsInMagazine);
            StartReloadCoroutine(weapon);
        }

        [Server]
        private void ReloadFinished(Weapon weapon)
        {
            weapon.IsReloading = false;
        }

        [Server]
        private void Shoot(Weapon weapon)
        {
            _audioSource.clip = weapon.ShootingAudioClip;
            _audioSource.volume = weapon.ShootingVolume;
            _audioSource.Play();
            
            weapon.BulletsInMagazine -= weapon.BulletsPerShot;
            if (weapon.BulletsInMagazine <= 0)
                weapon.BulletsInMagazine = 0;
            weapon.IsReady = false;
            weapon.RecoilModifier += weapon.StepRecoil;
            SendWeaponState(weapon.ID, weapon.BulletsInMagazine);
            StartShootCoroutines(weapon);
        }

        [Server]
        private void ApplyRaycast(Ray ray, Weapon weapon)
        {
            var x = Math.Abs(weapon.RecoilModifier) < 0.00001
                ? 0
                : UnityEngine.Random.Range(-weapon.BaseRecoil, weapon.BaseRecoil) * (weapon.RecoilModifier + 1);
            var y = Math.Abs(weapon.RecoilModifier) < 0.00001
                ? 0
                : UnityEngine.Random.Range(-weapon.BaseRecoil, weapon.BaseRecoil) * (weapon.RecoilModifier + 1);
            ray = new Ray(ray.origin, ray.direction + new Vector3(x, y));
            var raycastResult = Physics.Raycast(ray, out var rayHit, weapon.Range);
            if (!raycastResult) return;
            if (rayHit.collider.CompareTag("Head"))
            {
                var connection = rayHit.collider.gameObject.GetComponentInParent<NetworkIdentity>().connectionToClient;
                GetComponent<HealthSynchronization>().Damage(connection, (int) (weapon.HeadMultiplier * weapon.Damage));
            }

            if (rayHit.collider.CompareTag("Leg"))
            {
                var connection = rayHit.collider.gameObject.GetComponentInParent<NetworkIdentity>().connectionToClient;
                GetComponent<HealthSynchronization>().Damage(connection, (int) (weapon.LegMultiplier * weapon.Damage));
            }

            if (rayHit.collider.CompareTag("Chest"))
            {
                var connection = rayHit.collider.gameObject.GetComponentInParent<NetworkIdentity>().connectionToClient;
                GetComponent<HealthSynchronization>()
                    .Damage(connection, (int) (weapon.ChestMultiplier * weapon.Damage));
            }

            if (rayHit.collider.CompareTag("Arm"))
            {
                var connection = rayHit.collider.gameObject.GetComponentInParent<NetworkIdentity>().connectionToClient;
                GetComponent<HealthSynchronization>().Damage(connection, (int) (weapon.ArmMultiplier * weapon.Damage));
            }

            if (rayHit.collider.CompareTag("Chunk"))
            {
                _particleFactory.CreateBulletHole(rayHit.point, Quaternion.Euler(rayHit.normal.y * -90,
                    rayHit.normal.x * 90 + rayHit.normal.z * -180, 0));
            }
        }

        [Server]
        private bool CanShoot(Weapon weapon) => weapon.IsReady && !weapon.IsReloading && weapon.BulletsInMagazine > 0;

        [Server]
        private bool CanReload(Weapon weapon) => weapon.BulletsInMagazine < weapon.MagazineSize &&
                                                 !weapon.IsReloading && weapon.TotalBullets > 0;

        private static IEnumerator WaitForSeconds(Action action, float timeInSeconds)
        {
            yield return new WaitForSeconds(timeInSeconds);
            action();
        }
    }
}
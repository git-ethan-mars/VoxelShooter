using System;
using System.Collections;
using Data;
using Infrastructure.Factory;
using Mirror;
using UnityEngine;

namespace Networking.Synchronization
{
    public class RangeWeaponSynchronization : NetworkBehaviour
    {
        private ServerData _serverData;
        private IParticleFactory _particleFactory;

        public void Construct(IParticleFactory particleFactory, ServerData serverData)
        {
            _serverData = serverData;
            _particleFactory = particleFactory;
        }

        [Command]
        public void CmdShootSingle(Ray ray, int weaponId, NetworkConnectionToClient conn = null)
        {
            var weapon = _serverData.GetPlayerData(conn!.connectionId).RangeWeaponsById[weaponId];
            if (!CanShoot(weapon) || weapon.IsAutomatic) return;
            ApplyRaycast(ray, weapon);
            Shoot(weapon);
        }

        [Command]
        public void CmdShootAutomatic(Ray ray, int weaponId, NetworkConnectionToClient conn = null)
        {
            var weapon = _serverData.GetPlayerData(conn!.connectionId).RangeWeaponsById[weaponId];
            if (!CanShoot(weapon) || !weapon.IsAutomatic) return;
            ApplyRaycast(ray, weapon);
            Shoot(weapon);
        }

        [Command]
        public void CmdReload(int weaponId, NetworkConnectionToClient conn = null)
        {
            var weapon = _serverData.GetPlayerData(conn!.connectionId).RangeWeaponsById[weaponId];
            if (CanReload(weapon))
            {
                Reload(weapon);
            }
        }


        [TargetRpc]
        private void SendWeaponState(int weaponId, int bulletsInMagazine)
        {
            var weapon = GetComponent<PlayerLogic.Inventory>().RangeWeapons[weaponId];
            var audioSource = GetComponent<AudioSource>();
            audioSource.clip = weapon.ShootingAudioClip;
            audioSource.volume = weapon.ShootingVolume;
            audioSource.Play();
            weapon.BulletsInMagazine = bulletsInMagazine;
        }

        [TargetRpc]
        private void SendReload(int weaponId, int totalBullets, int bulletsInMagazine)
        {
            var weapon = GetComponent<PlayerLogic.Inventory>().RangeWeapons[weaponId];
            var audioSource = GetComponent<AudioSource>();
            audioSource.clip = weapon.ReloadingAudioClip;
            audioSource.volume = weapon.ReloadingVolume;
            audioSource.Play();
            weapon.TotalBullets = totalBullets;
            weapon.BulletsInMagazine = bulletsInMagazine;
        }

        [Server]
        private void StartShootCoroutines(RangeWeaponData rangeWeapon)
        {
            StartCoroutine(WaitForSeconds(() => ResetShoot(rangeWeapon), rangeWeapon.TimeBetweenShooting));
            StartCoroutine(WaitForSeconds(() => ResetRecoil(rangeWeapon), rangeWeapon.ResetTimeRecoil));
            /*if (weapon.BulletsPerShot > 0 && weapon.BulletsInMagazine > 0)
            {
                Shoot(weapon);
                weapon.BulletsPerShot--;
            }*/
        }

        [Server]
        private void StartReloadCoroutine(RangeWeaponData rangeWeapon)
        {
            StartCoroutine(WaitForSeconds(() => ReloadFinished(rangeWeapon), rangeWeapon.ReloadTime));
        }

        [Server]
        private void ResetRecoil(RangeWeaponData rangeWeapon)
        {
            rangeWeapon.RecoilModifier -= rangeWeapon.StepRecoil;
        }

        [Server]
        private void ResetShoot(RangeWeaponData rangeWeapon)
        {
            rangeWeapon.IsReady = true;
        }

        [Server]
        private void Reload(RangeWeaponData rangeWeapon)
        {
            rangeWeapon.IsReloading = true;
            if (rangeWeapon.TotalBullets + rangeWeapon.BulletsInMagazine - rangeWeapon.MagazineSize <= 0)
            {
                rangeWeapon.BulletsInMagazine += rangeWeapon.TotalBullets;
                rangeWeapon.TotalBullets = 0;
            }
            else
            {
                rangeWeapon.TotalBullets -= rangeWeapon.MagazineSize - rangeWeapon.BulletsInMagazine;
                rangeWeapon.BulletsInMagazine = rangeWeapon.MagazineSize;
            }
            SendReload(rangeWeapon.ID, rangeWeapon.TotalBullets, rangeWeapon.BulletsInMagazine);
            StartReloadCoroutine(rangeWeapon);
        }

        [Server]
        private void ReloadFinished(RangeWeaponData rangeWeapon)
        {
            rangeWeapon.IsReloading = false;
        }

        [Server]
        private void Shoot(RangeWeaponData rangeWeapon)
        {
            rangeWeapon.BulletsInMagazine -= rangeWeapon.BulletsPerShot;
            if (rangeWeapon.BulletsInMagazine <= 0)
                rangeWeapon.BulletsInMagazine = 0;
            rangeWeapon.IsReady = false;
            rangeWeapon.RecoilModifier += rangeWeapon.StepRecoil;
            SendWeaponState(rangeWeapon.ID, rangeWeapon.BulletsInMagazine);
            StartShootCoroutines(rangeWeapon);
        }

        [Server]
        private void ApplyRaycast(Ray ray, RangeWeaponData rangeWeapon)
        {
            var x = Math.Abs(rangeWeapon.RecoilModifier) < 0.00001
                ? 0
                : UnityEngine.Random.Range(-rangeWeapon.BaseRecoil, rangeWeapon.BaseRecoil) * (rangeWeapon.RecoilModifier + 1);
            var y = Math.Abs(rangeWeapon.RecoilModifier) < 0.00001
                ? 0
                : UnityEngine.Random.Range(-rangeWeapon.BaseRecoil, rangeWeapon.BaseRecoil) * (rangeWeapon.RecoilModifier + 1);
            ray = new Ray(ray.origin, ray.direction + new Vector3(x, y));
            var raycastResult = Physics.Raycast(ray, out var rayHit, rangeWeapon.Range);
            if (!raycastResult) return;
            if (rayHit.collider.CompareTag("Head"))
            {
                ShootImpact(rayHit, (int) (rangeWeapon.HeadMultiplier * rangeWeapon.Damage));
            }

            if (rayHit.collider.CompareTag("Leg"))
            {
                ShootImpact(rayHit, (int) (rangeWeapon.LegMultiplier * rangeWeapon.Damage));
            }

            if (rayHit.collider.CompareTag("Chest"))
            {
                ShootImpact(rayHit, (int) (rangeWeapon.ChestMultiplier * rangeWeapon.Damage));
            }

            if (rayHit.collider.CompareTag("Arm"))
            {
                ShootImpact(rayHit, (int) (rangeWeapon.ArmMultiplier * rangeWeapon.Damage));
            }

            if (rayHit.collider.CompareTag("Chunk"))
            {
                _particleFactory.CreateBulletHole(rayHit.point, Quaternion.Euler(rayHit.normal.y * -90,
                    rayHit.normal.x * 90 + rayHit.normal.z * -180, 0));
            }
        }

        private void ShootImpact(RaycastHit rayHit, int damage)
        {
            var connection = rayHit.collider.gameObject.GetComponentInParent<NetworkIdentity>().connectionToClient;
            GetComponent<HealthSynchronization>().Damage(connection, damage);
            _particleFactory.CreateBlood(rayHit.point);
        }

        [Server]
        private bool CanShoot(RangeWeaponData rangeWeapon) => rangeWeapon.IsReady && !rangeWeapon.IsReloading && rangeWeapon.BulletsInMagazine > 0;

        [Server]
        private bool CanReload(RangeWeaponData rangeWeapon) => rangeWeapon.BulletsInMagazine < rangeWeapon.MagazineSize &&
                                                 !rangeWeapon.IsReloading && rangeWeapon.TotalBullets > 0;

        private static IEnumerator WaitForSeconds(Action action, float timeInSeconds)
        {
            yield return new WaitForSeconds(timeInSeconds);
            action();
        }
    }
}
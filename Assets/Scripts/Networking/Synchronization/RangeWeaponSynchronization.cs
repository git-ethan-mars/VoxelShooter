using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using Infrastructure.AssetManagement;
using Infrastructure.Factory;
using Inventory;
using Mirror;
using Networking.Messages;
using UnityEngine;

namespace Networking.Synchronization
{
    public class RangeWeaponSynchronization : NetworkBehaviour
    {
        private ServerData _serverData;
        private IParticleFactory _particleFactory;
        private IAssetProvider _assets;
        private List<AudioClip> _audioClips;

        public void Construct(IParticleFactory particleFactory, IAssetProvider assets, ServerData serverData)
        {
            _serverData = serverData;
            _particleFactory = particleFactory;
            _audioClips = assets.LoadAll<AudioClip>("Audio/Sounds").ToList();
        }

        [Command]
        public void CmdShootSingle(Ray ray, int weaponId, NetworkConnectionToClient connection = null)
        {
            var weapon = _serverData.GetPlayerData(connection).RangeWeaponsById[weaponId];
            if (!CanShoot(weapon) || weapon.IsAutomatic) return;
            Shoot(weapon, connection);
            for (var i = 0; i < weapon.BulletsPerShot; i++)
                ApplyRaycast(connection, ray, weapon);
            GetComponent<SoundSynchronization>().PlayAudioClip(connection!.identity,
                _audioClips.FindIndex(audioClip => audioClip == weapon.ShootAudioClip), weapon.ShootingVolume);
        }

        [Command]
        public void CmdShootAutomatic(Ray ray, int weaponId, NetworkConnectionToClient connection = null)
        {
            var weapon = _serverData.GetPlayerData(connection).RangeWeaponsById[weaponId];
            if (!CanShoot(weapon) || !weapon.IsAutomatic) return;
            Shoot(weapon, connection);
            for (var i = 0; i < weapon.BulletsPerShot; i++)
                ApplyRaycast(connection, ray, weapon);
            GetComponent<SoundSynchronization>().PlayAudioClip(connection!.identity,
                _audioClips.FindIndex(audioClip => audioClip == weapon.ShootAudioClip), weapon.ShootingVolume);
        }

        [Command]
        public void CmdReload(int weaponId, NetworkConnectionToClient connection = null)
        {
            var weapon = _serverData.GetPlayerData(connection).RangeWeaponsById[weaponId];
            if (!CanReload(weapon)) return;
            Reload(weapon, connection);
            GetComponent<SoundSynchronization>().PlayAudioClip(connection!.identity,
                _audioClips.FindIndex(audioClip => audioClip == weapon.ReloadingAudioClip), weapon.ReloadingVolume);
        }
        
        [Server]
        private void StartShootCoroutines(RangeWeaponData rangeWeapon)
        {
            StartCoroutine(WaitForSeconds(() => ResetShoot(rangeWeapon), rangeWeapon.TimeBetweenShooting));
            StartCoroutine(WaitForSeconds(() => ResetRecoil(rangeWeapon), rangeWeapon.ResetTimeRecoil));
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
        private void Reload(RangeWeaponData rangeWeapon, NetworkConnectionToClient connection)
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

            connection.Send(new ReloadResult(rangeWeapon.ID, rangeWeapon.TotalBullets, rangeWeapon.BulletsInMagazine));
            StartReloadCoroutine(rangeWeapon);
        }

        [Server]
        private void ReloadFinished(RangeWeaponData rangeWeapon)
        {
            rangeWeapon.IsReloading = false;
        }

        [Server]
        private void Shoot(RangeWeaponData rangeWeapon, NetworkConnectionToClient connection)
        {
            rangeWeapon.BulletsInMagazine -= 1;
            if (rangeWeapon.BulletsInMagazine <= 0)
                rangeWeapon.BulletsInMagazine = 0;
            rangeWeapon.IsReady = false;
            rangeWeapon.RecoilModifier += rangeWeapon.StepRecoil;
            connection.Send(new ShootResult(rangeWeapon.ID, rangeWeapon.BulletsInMagazine));
            StartShootCoroutines(rangeWeapon);
        }

        [Server]
        private void ApplyRaycast(NetworkConnectionToClient source, Ray ray,
            RangeWeaponData rangeWeapon)
        {
            var x = Math.Abs(rangeWeapon.RecoilModifier) < 0.00001
                ? 0
                : UnityEngine.Random.Range(-rangeWeapon.BaseRecoil, rangeWeapon.BaseRecoil) *
                  (rangeWeapon.RecoilModifier + 1);
            var y = Math.Abs(rangeWeapon.RecoilModifier) < 0.00001
                ? 0
                : UnityEngine.Random.Range(-rangeWeapon.BaseRecoil, rangeWeapon.BaseRecoil) *
                  (rangeWeapon.RecoilModifier + 1);
            var z = Math.Abs(rangeWeapon.RecoilModifier) < 0.00001
                ? 0
                : UnityEngine.Random.Range(-rangeWeapon.BaseRecoil, rangeWeapon.BaseRecoil) *
                  (rangeWeapon.RecoilModifier + 1);
            ray = new Ray(ray.origin, ray.direction + new Vector3(x, y, z));
            var raycastResult = Physics.Raycast(ray, out var rayHit, rangeWeapon.Range);
            if (!raycastResult) return;
            if (rayHit.collider.CompareTag("Head"))
            {
                ShootImpact(source, rayHit, (int) (rangeWeapon.HeadMultiplier * rangeWeapon.Damage));
            }

            if (rayHit.collider.CompareTag("Leg"))
            {
                ShootImpact(source, rayHit, (int) (rangeWeapon.LegMultiplier * rangeWeapon.Damage));
            }

            if (rayHit.collider.CompareTag("Chest"))
            {
                ShootImpact(source, rayHit, (int) (rangeWeapon.ChestMultiplier * rangeWeapon.Damage));
            }

            if (rayHit.collider.CompareTag("Arm"))
            {
                ShootImpact(source, rayHit, (int) (rangeWeapon.ArmMultiplier * rangeWeapon.Damage));
            }

            if (rayHit.collider.CompareTag("Chunk"))
            {
                var vector = Vector3Int.FloorToInt(rayHit.point - rayHit.normal / 2);
                var block = _serverData.Map.GetBlockByGlobalPosition(new Vector3Int(vector.x, vector.y, vector.z));
                _particleFactory.CreateBulletImpact(rayHit.point, Quaternion.Euler(rayHit.normal.y * -90, 
                    rayHit.normal.x * 90 + (rayHit.normal.z == -1 ? 180 : 0), 0), block.Color);
            }
        }

        private void ShootImpact(NetworkConnectionToClient source, RaycastHit rayHit, int damage)
        {
            var receiver = rayHit.collider.gameObject.GetComponentInParent<NetworkIdentity>().connectionToClient;
            if (source != receiver)
            {
                GetComponent<HealthSynchronization>().Damage(source, receiver, damage);
                _particleFactory.CreateBlood(rayHit.point);
            }
        }


        [Server]
        private bool CanShoot(RangeWeaponData rangeWeapon) =>
            rangeWeapon.IsReady && !rangeWeapon.IsReloading && rangeWeapon.BulletsInMagazine > 0;

        [Server]
        private bool CanReload(RangeWeaponData rangeWeapon) =>
            rangeWeapon.BulletsInMagazine < rangeWeapon.MagazineSize &&
            !rangeWeapon.IsReloading && rangeWeapon.TotalBullets > 0;

        private static IEnumerator WaitForSeconds(Action action, float timeInSeconds)
        {
            yield return new WaitForSeconds(timeInSeconds);
            action();
        }
    }
}
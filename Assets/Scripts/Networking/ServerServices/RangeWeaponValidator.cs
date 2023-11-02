using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Infrastructure;
using Infrastructure.Factory;
using Inventory;
using Mirror;
using Networking.Messages.Responses;
using UnityEngine;

namespace Networking.ServerServices
{
    public class RangeWeaponValidator
    {
        private readonly IServer _server;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly IParticleFactory _particleFactory;

        public RangeWeaponValidator(IServer server, ICoroutineRunner coroutineRunner, IParticleFactory particleFactory)
        {
            _server = server;
            _coroutineRunner = coroutineRunner;
            _particleFactory = particleFactory;
        }

        public void Shoot(NetworkConnectionToClient connection, Ray ray, bool requestIsButtonHolding)
        {
            var playerData = _server.Data.GetPlayerData(connection);
            if (!playerData.RangeWeaponsById.TryGetValue(playerData.ItemIds[playerData.InventorySlotId],
                    out var weapon))
            {
                return;
            }

            if (!CanShoot(weapon) || requestIsButtonHolding != weapon.IsAutomatic)
            {
                return;
            }

            for (var i = 0; i < weapon.BulletsPerShot; i++)
            {
                ApplyRaycast(connection, ray, weapon);
            }

            UpdateWeaponState(weapon);
            connection.Send(new ShootResultResponse(weapon.ID, weapon.BulletsInMagazine));
            StartShootCoroutines(weapon);

            //GetComponent<SoundSynchronization>().PlayAudioClip(connection!.identity,
            //    _audioClips.FindIndex(audioClip => audioClip == weapon.ShootAudioClip), weapon.ShootingVolume);
        }

        public void Reload(NetworkConnectionToClient connection)
        {
            var playerData = _server.Data.GetPlayerData(connection);
            if (!playerData.RangeWeaponsById.TryGetValue(playerData.ItemIds[playerData.InventorySlotId],
                    out var weapon))
            {
                return;
            }

            if (!CanReload(weapon))
            {
                return;
            }

            ReloadInternal(weapon);
            connection.Send(new ReloadResultResponse(weapon.ID, weapon.TotalBullets,
                weapon.BulletsInMagazine));
            StartReloadCoroutine(weapon);
            //GetComponent<SoundSynchronization>().PlayAudioClip(connection!.identity,
            //    _audioClips.FindIndex(audioClip => audioClip == weapon.ReloadingAudioClip), weapon.ReloadingVolume);
        }

        private void StartShootCoroutines(RangeWeaponData rangeWeapon)
        {
            _coroutineRunner.StartCoroutine(Utils.DoActionAfterDelay(() => ResetShoot(rangeWeapon), rangeWeapon.TimeBetweenShooting));
            _coroutineRunner.StartCoroutine(Utils.DoActionAfterDelay(() => ResetRecoil(rangeWeapon), rangeWeapon.ResetTimeRecoil));
        }


        private void StartReloadCoroutine(RangeWeaponData rangeWeapon)
        {
            _coroutineRunner.StartCoroutine(Utils.DoActionAfterDelay(() => ReloadFinished(rangeWeapon), rangeWeapon.ReloadTime));
        }

        private void ResetRecoil(RangeWeaponData rangeWeapon)
        {
            if (rangeWeapon is not null)
                rangeWeapon.RecoilModifier -= rangeWeapon.StepRecoil;
        }

        private void ResetShoot(RangeWeaponData rangeWeapon)
        {
            if (rangeWeapon is not null)
                rangeWeapon.IsReady = true;
        }

        private void ReloadInternal(RangeWeaponData rangeWeapon)
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
        }

        private void ReloadFinished(RangeWeaponData rangeWeapon)
        {
            if (rangeWeapon is not null)
                rangeWeapon.IsReloading = false;
        }

        private void UpdateWeaponState(RangeWeaponData rangeWeapon)
        {
            rangeWeapon.BulletsInMagazine -= 1;
            if (rangeWeapon.BulletsInMagazine <= 0)
                rangeWeapon.BulletsInMagazine = 0;
            rangeWeapon.IsReady = false;
            rangeWeapon.RecoilModifier += rangeWeapon.StepRecoil;
        }

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
            var raycastResult = Physics.Raycast(ray, out var rayHit, rangeWeapon.Range, Constants.attackMask);
            if (!raycastResult)
            {
                return;
            }

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
                var block = _server.MapProvider.GetBlockByGlobalPosition(
                    Vector3Int.FloorToInt(rayHit.point - rayHit.normal / 2));
                _particleFactory.CreateBulletImpact(rayHit.point, Quaternion.Euler(rayHit.normal.y * -90,
                    rayHit.normal.x * 90 + (rayHit.normal.z == -1 ? 180 : 0), 0), block.Color);
            }
        }

        private void ShootImpact(NetworkConnectionToClient source, RaycastHit rayHit, int damage)
        {
            var receiver = rayHit.collider.gameObject.GetComponentInParent<NetworkIdentity>().connectionToClient;
            if (source != receiver)
            {
                _server.Damage(source, receiver, damage);
                _particleFactory.CreateBlood(rayHit.point, Quaternion.LookRotation(rayHit.normal));
            }
        }

        private bool CanShoot(RangeWeaponData rangeWeapon)
        {
            return rangeWeapon.IsReady && !rangeWeapon.IsReloading && rangeWeapon.BulletsInMagazine > 0;
        }

        private bool CanReload(RangeWeaponData rangeWeapon)
        {
            return rangeWeapon.BulletsInMagazine < rangeWeapon.MagazineSize &&
                   !rangeWeapon.IsReloading && rangeWeapon.TotalBullets > 0;
        }
    }
}
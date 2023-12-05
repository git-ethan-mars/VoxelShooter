using System;
using System.Collections;
using Data;
using Explosions;
using Infrastructure;
using Infrastructure.Factory;
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
        private readonly LineDamageArea _lineDamageArea;

        public RangeWeaponValidator(IServer server, ICoroutineRunner coroutineRunner, IParticleFactory particleFactory)
        {
            _server = server;
            _coroutineRunner = coroutineRunner;
            _particleFactory = particleFactory;
            _lineDamageArea = new LineDamageArea(_server.MapProvider);
        }

        public void Shoot(NetworkConnectionToClient connection, Ray ray, bool requestIsButtonHolding)
        {
            var playerData = _server.Data.GetPlayerData(connection);
            var rangeWeapon = (RangeWeaponItem) playerData.Items[playerData.SelectedSlotIndex];
            var rangeWeaponData = (RangeWeaponData) playerData.ItemData[playerData.SelectedSlotIndex];

            if (!CanShoot(rangeWeaponData) || requestIsButtonHolding != rangeWeapon.isAutomatic)
            {
                return;
            }

            for (var i = 0; i < rangeWeapon.bulletsPerTap; i++)
            {
                ApplyRaycast(connection, ray, rangeWeapon, rangeWeaponData);
            }

            rangeWeaponData.BulletsInMagazine -= 1;
            if (rangeWeaponData.BulletsInMagazine <= 0)
            {
                rangeWeaponData.BulletsInMagazine = 0;
            }

            connection.Send(new ShootResultResponse(rangeWeaponData.BulletsInMagazine));
            _coroutineRunner.StartCoroutine(ResetShoot(connection, rangeWeapon, rangeWeaponData));
            _coroutineRunner.StartCoroutine(ResetRecoil(connection, rangeWeapon, rangeWeaponData));
        }

        public void Reload(NetworkConnectionToClient connection)
        {
            var playerData = _server.Data.GetPlayerData(connection);
            var rangeWeapon = (RangeWeaponItem) playerData.Items[playerData.SelectedSlotIndex];
            var rangeWeaponData = (RangeWeaponData) playerData.ItemData[playerData.SelectedSlotIndex];
            if (!CanReload(rangeWeapon, rangeWeaponData))
            {
                return;
            }

            _coroutineRunner.StartCoroutine(ReloadInternal(connection, rangeWeapon, rangeWeaponData));
        }

        private IEnumerator ReloadInternal(NetworkConnectionToClient connection, RangeWeaponItem configure,
            RangeWeaponData data)
        {
            data.IsReloading = true;
            var waitReloading = new WaitWithoutSlotChange(_server, connection, configure.reloadTime);
            yield return waitReloading;
            if (!waitReloading.CompletedSuccessfully)
            {
                data.IsReloading = false;
                yield break;
            }

            data.IsReloading = false;
            if (data.TotalBullets + data.BulletsInMagazine - configure.magazineSize <= 0)
            {
                data.BulletsInMagazine += data.TotalBullets;
                data.TotalBullets = 0;
            }
            else
            {
                data.TotalBullets -= configure.magazineSize - data.BulletsInMagazine;
                data.BulletsInMagazine = configure.magazineSize;
            }

            var playerData = _server.Data.GetPlayerData(connection);
            connection.Send(new ReloadResultResponse(playerData.SelectedSlotIndex, data.TotalBullets,
                data.BulletsInMagazine));
        }

        private IEnumerator ResetRecoil(NetworkConnectionToClient connection, RangeWeaponItem configure,
            RangeWeaponData data)
        {
            data.RecoilModifier += configure.stepRecoil;
            var waitForRecoilReset = new WaitWithoutSlotChange(_server, connection, configure.resetTimeRecoil);
            while (true)
            {
                yield return waitForRecoilReset;
                if (waitForRecoilReset.CompletedSuccessfully || waitForRecoilReset.IsAborted)
                {
                    break;
                }

                waitForRecoilReset = new WaitWithoutSlotChange(_server, connection, configure.resetTimeRecoil);
            }

            if (waitForRecoilReset.CompletedSuccessfully)
            {
                data.RecoilModifier -= configure.stepRecoil;
            }
        }

        private IEnumerator ResetShoot(NetworkConnectionToClient connection, RangeWeaponItem configure,
            RangeWeaponData data)
        {
            data.IsReady = false;
            var waitForShootReset = new WaitWithoutSlotChange(_server, connection, configure.timeBetweenShooting);
            while (true)
            {
                yield return waitForShootReset;
                if (waitForShootReset.CompletedSuccessfully || waitForShootReset.IsAborted)
                {
                    break;
                }

                waitForShootReset = new WaitWithoutSlotChange(_server, connection, configure.timeBetweenShooting);
            }

            if (waitForShootReset.CompletedSuccessfully)
            {
                data.IsReady = true;
            }
        }

        private void ApplyRaycast(NetworkConnectionToClient source, Ray ray,
            RangeWeaponItem configure, RangeWeaponData data)
        {
            var x = Math.Abs(data.RecoilModifier) < Constants.Epsilon
                ? 0
                : UnityEngine.Random.Range(-configure.baseRecoil, configure.baseRecoil) *
                  (data.RecoilModifier + 1);
            var y = Math.Abs(data.RecoilModifier) < Constants.Epsilon
                ? 0
                : UnityEngine.Random.Range(-configure.baseRecoil, configure.baseRecoil) *
                  (data.RecoilModifier + 1);
            var z = Math.Abs(data.RecoilModifier) < Constants.Epsilon
                ? 0
                : UnityEngine.Random.Range(-configure.baseRecoil, configure.baseRecoil) *
                  (data.RecoilModifier + 1);
            ray = new Ray(ray.origin, ray.direction + new Vector3(x, y, z));
            var raycastResult = Physics.Raycast(ray, out var rayHit, configure.range, Constants.attackMask);
            if (!raycastResult)
            {
                return;
            }

            if (rayHit.collider.CompareTag("Head"))
            {
                ShootImpact(source, rayHit, (int) (configure.headMultiplier * configure.damage));
            }

            if (rayHit.collider.CompareTag("Leg"))
            {
                ShootImpact(source, rayHit, (int) (configure.legMultiplier * configure.damage));
            }

            if (rayHit.collider.CompareTag("Chest"))
            {
                ShootImpact(source, rayHit, (int) (configure.chestMultiplier * configure.damage));
            }

            if (rayHit.collider.CompareTag("Arm"))
            {
                ShootImpact(source, rayHit, (int) (configure.armMultiplier * configure.damage));
            }

            if (rayHit.collider.CompareTag("Chunk"))
            {
                var blockPosition = Vector3Int.FloorToInt(rayHit.point - rayHit.normal / 2);
                var block = _server.MapProvider.GetBlockByGlobalPosition(blockPosition);
                _server.BlockHealthSystem.DamageBlock(blockPosition,1, configure.damage, _lineDamageArea);
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

        private bool CanReload(RangeWeaponItem configure, RangeWeaponData data)
        {
            return data.BulletsInMagazine < configure.magazineSize &&
                   !data.IsReloading && data.TotalBullets > 0;
        }
    }
}
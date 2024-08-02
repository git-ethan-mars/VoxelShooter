using System;
using System.Collections;
using System.Collections.Generic;
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
        private readonly BlockDestructionBehaviour _blockDestructionBehaviour;
        private readonly AudioService _audioService;
        private readonly MuzzleFlashService _muzzleFlashService;

        public RangeWeaponValidator(IServer server, CustomNetworkManager networkManager, AudioService audioService,
            MuzzleFlashService muzzleFlashService)
        {
            _server = server;
            _coroutineRunner = networkManager;
            _particleFactory = networkManager.ParticleFactory;
            var blockArea = new SingleBlockArea(server.MapProvider);
            _blockDestructionBehaviour =
                new BlockDestructionBehaviour(server, blockArea);
            _audioService = audioService;
            _muzzleFlashService = muzzleFlashService;
        }

        public void Shoot(NetworkConnectionToClient connection, Ray ray, bool requestIsButtonHolding)
        {
            var playerData = _server.GetPlayerData(connection);
            var rangeWeapon = (RangeWeaponItem) playerData.SelectedItem;
            var rangeWeaponData = (RangeWeaponItemData) playerData.SelectedItemData;

            if (rangeWeaponData.BulletsInMagazine == 0 && playerData.HasContinuousSound)
            {
                _audioService.StopContinuousSound(connection.identity);
                playerData.HasContinuousSound = false;
                _muzzleFlashService.StopMuzzleFlash(connection.identity);
            }

            if (!CanShoot(rangeWeaponData) || requestIsButtonHolding != rangeWeapon.isAutomatic)
            {
                return;
            }

            var bulletImpactColors = new Dictionary<Vector3Int, Color32>();
            for (var i = 0; i < rangeWeapon.bulletsPerTap; i++)
            {
                ApplyRaycast(connection, ray, rangeWeapon, rangeWeaponData, bulletImpactColors);
                rangeWeaponData.RecoilModifier += rangeWeapon.stepRecoil;
            }

            rangeWeaponData.BulletsInMagazine -= 1;
            connection.Send(new ShootResultResponse(playerData.SelectedSlotIndex, rangeWeaponData.BulletsInMagazine));
            _coroutineRunner.StartCoroutine(ResetShoot(connection, rangeWeapon, rangeWeaponData));
            _coroutineRunner.StartCoroutine(ResetRecoil(connection, rangeWeapon, rangeWeaponData));
            
            if (rangeWeapon.isAutomatic)
            {
                _audioService.StartContinuousAudio(rangeWeapon.shootingSound, connection.identity);
                playerData.HasContinuousSound = true;
            }
            else
            {
                _audioService.SendAudio(rangeWeapon.shootingSound, connection.identity);
            }
            _muzzleFlashService.StartMuzzleFlash(connection.identity);
        }

        public void CancelShoot(NetworkConnectionToClient connection)
        {
            var playerData = _server.GetPlayerData(connection);
            _audioService.StopContinuousSound(connection.identity);
            playerData.HasContinuousSound = false;
            _muzzleFlashService.StopMuzzleFlash(connection.identity);
        }

        public void Reload(NetworkConnectionToClient connection)
        {
            var playerData = _server.GetPlayerData(connection);
            var rangeWeapon = (RangeWeaponItem) playerData.SelectedItem;
            var rangeWeaponData = (RangeWeaponItemData) playerData.SelectedItemData;
            if (!CanReload(rangeWeapon, rangeWeaponData))
            {
                return;
            }

            _coroutineRunner.StartCoroutine(ReloadInternal(connection, rangeWeapon, rangeWeaponData));
            _audioService.SendAudio(rangeWeapon.reloadingSound, connection.identity);
            _muzzleFlashService.StopMuzzleFlash(connection.identity);
        }

        private IEnumerator ReloadInternal(NetworkConnectionToClient connection, RangeWeaponItem configure,
            RangeWeaponItemData itemData)
        {
            itemData.IsReloading = true;
            var waitReloading = new WaitWithoutSlotChange(_server, connection, configure.reloadTime);
            yield return waitReloading;
            if (!waitReloading.CompletedSuccessfully)
            {
                itemData.IsReloading = false;
                yield break;
            }

            itemData.IsReloading = false;
            if (itemData.TotalBullets + itemData.BulletsInMagazine - configure.magazineSize <= 0)
            {
                itemData.BulletsInMagazine += itemData.TotalBullets;
                itemData.TotalBullets = 0;
            }
            else
            {
                itemData.TotalBullets -= configure.magazineSize - itemData.BulletsInMagazine;
                itemData.BulletsInMagazine = configure.magazineSize;
            }

            var playerData = _server.GetPlayerData(connection);
            connection.Send(new ReloadResultResponse(playerData.SelectedSlotIndex, itemData.TotalBullets,
                itemData.BulletsInMagazine));
        }

        private IEnumerator ResetRecoil(NetworkConnectionToClient connection, RangeWeaponItem configure,
            RangeWeaponItemData itemData)
        {
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
                itemData.RecoilModifier -= configure.stepRecoil * configure.bulletsPerTap;
            }
        }

        private IEnumerator ResetShoot(NetworkConnectionToClient connection, RangeWeaponItem configure,
            RangeWeaponItemData itemData)
        {
            itemData.IsReady = false;
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
                itemData.IsReady = true;
            }
        }

        private void ApplyRaycast(NetworkConnectionToClient source, Ray ray,
            RangeWeaponItem configure, RangeWeaponItemData itemData, Dictionary<Vector3Int, Color32> bulletImpactColors)
        {
            var x = Math.Abs(itemData.RecoilModifier) < Constants.Epsilon
                ? 0
                : UnityEngine.Random.Range(-configure.baseRecoil, configure.baseRecoil) *
                  (itemData.RecoilModifier + 1);
            var y = Math.Abs(itemData.RecoilModifier) < Constants.Epsilon
                ? 0
                : UnityEngine.Random.Range(-configure.baseRecoil, configure.baseRecoil) *
                  (itemData.RecoilModifier + 1);
            var z = Math.Abs(itemData.RecoilModifier) < Constants.Epsilon
                ? 0
                : UnityEngine.Random.Range(-configure.baseRecoil, configure.baseRecoil) *
                  (itemData.RecoilModifier + 1);
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

                if (block.IsSolid())
                {
                    bulletImpactColors[blockPosition] = block.color;
                    _blockDestructionBehaviour.DamageBlocks(blockPosition, configure.damage);
                }

                if (bulletImpactColors.ContainsKey(blockPosition))
                {
                    var bullet = _particleFactory.CreateBulletImpact(rayHit.point, Quaternion.Euler(
                            rayHit.normal.y * -90,
                            rayHit.normal.x * 90 + (rayHit.normal.z == -1 ? 180 : 0), 0),
                        bulletImpactColors[blockPosition]);
                    NetworkServer.Spawn(bullet);
                }
            }
        }

        private void ShootImpact(NetworkConnectionToClient source, RaycastHit rayHit, int damage)
        {
            var receiver = rayHit.collider.gameObject.GetComponentInParent<NetworkIdentity>().connectionToClient;
            if (source != receiver)
            {
                _server.Damage(source, receiver, damage);
                var blood = _particleFactory.CreateBlood(rayHit.point, Quaternion.LookRotation(rayHit.normal));
                NetworkServer.Spawn(blood);
            }
        }

        private bool CanShoot(RangeWeaponItemData rangeWeaponItem)
        {
            return rangeWeaponItem.IsReady && !rangeWeaponItem.IsReloading && rangeWeaponItem.BulletsInMagazine > 0;
        }

        private bool CanReload(RangeWeaponItem configure, RangeWeaponItemData itemData)
        {
            return itemData.BulletsInMagazine < configure.magazineSize &&
                   !itemData.IsReloading && itemData.TotalBullets > 0;
        }
    }
}
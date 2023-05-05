using System;
using System.Collections;
using Data;
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

        public void Construct(IParticleFactory particleFactory, ServerData serverData)
        {
            _serverData = serverData;
            _particleFactory = particleFactory;
        }

        [Command]
        public void CmdShootSingle(Ray ray, int weaponId, NetworkConnectionToClient source = null)
        {
            var weapon = _serverData.GetPlayerData(source).RangeWeaponsById[weaponId];
            if (!CanShoot(weapon) || weapon.IsAutomatic) return;
            ApplyRaycast(source, ray, weapon);
            Shoot(weapon, source);
            SendSoundInRadius(weapon, source!.identity.gameObject.transform.position, Radius, SoundType.Shooting);
        }

        [Command]
        public void CmdShootAutomatic(Ray ray, int weaponId, NetworkConnectionToClient source = null)
        {
            var weapon = _serverData.GetPlayerData(source).RangeWeaponsById[weaponId];
            if (!CanShoot(weapon) || !weapon.IsAutomatic) return;
            ApplyRaycast(source, ray, weapon);
            Shoot(weapon, source);
            SendSoundInRadius(weapon, source!.identity.gameObject.transform.position, Radius, SoundType.Shooting);

        }

        [Command]
        public void CmdReload(int weaponId, NetworkConnectionToClient connection = null)
        {
            var weapon = _serverData.GetPlayerData(connection).RangeWeaponsById[weaponId];
            if (!CanReload(weapon)) return;
            Reload(weapon, connection);
            SendSoundInRadius(weapon, connection!.identity.gameObject.transform.position, Radius, SoundType.Reloading);
        }

        private const float Radius = 100;

        

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
            rangeWeapon.BulletsInMagazine -= rangeWeapon.BulletsPerShot;
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
                : UnityEngine.Random.Range(-rangeWeapon.BaseRecoil, rangeWeapon.BaseRecoil) * (rangeWeapon.RecoilModifier + 1);
            var y = Math.Abs(rangeWeapon.RecoilModifier) < 0.00001
                ? 0
                : UnityEngine.Random.Range(-rangeWeapon.BaseRecoil, rangeWeapon.BaseRecoil) * (rangeWeapon.RecoilModifier + 1);
            ray = new Ray(ray.origin, ray.direction + new Vector3(x, y));
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
                _particleFactory.CreateBulletHole(rayHit.point, Quaternion.Euler(rayHit.normal.y * -90,
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    rayHit.normal.x * 90 + (rayHit.normal.z == -1 ? 180 : 0), 0));
            }
        }

        private void ShootImpact(NetworkConnectionToClient source, RaycastHit rayHit, int damage)
        {
            var receiver = rayHit.collider.gameObject.GetComponentInParent<NetworkIdentity>().connectionToClient;
            GetComponent<HealthSynchronization>().Damage(source, receiver, damage);
            _particleFactory.CreateBlood(rayHit.point);
        }

        [Server]
        private void SendSoundInRadius(RangeWeaponData weapon, Vector3 sourcePosition, float radius, SoundType soundType)
        {
            foreach (var connection in NetworkServer.connections.Values)
            {
                if (!connection.identity) continue;
                var playerPosition = connection.identity.gameObject.transform.position;
                if (Vector3.Distance(sourcePosition, playerPosition) < radius)
                {
                    connection.identity.GetComponent<SoundSynchronization>().PlayAudioClip(weapon.ID, soundType);
                }
            }
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
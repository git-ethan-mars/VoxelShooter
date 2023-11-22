using Data;
using Explosions;
using Infrastructure;
using Infrastructure.Factory;
using Mirror;
using UnityEngine;

namespace Networking.ServerServices
{
    public class MeleeWeaponValidator
    {
        private readonly IServer _server;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly IParticleFactory _particleFactory;
        private readonly IExplosionArea _lineExplosionArea;

        public MeleeWeaponValidator(IServer server, ICoroutineRunner coroutineRunner, IParticleFactory particleFactory)
        {
            _server = server;
            _particleFactory = particleFactory;
            _coroutineRunner = coroutineRunner;
            _lineExplosionArea = new LineExplosionArea(_server.MapProvider);
        }

        public void Hit(NetworkConnectionToClient connection, Ray ray, bool isStrongHit)
        {
            var playerData = _server.Data.GetPlayerData(connection);
            if (!playerData.MeleeWeaponsById.TryGetValue(playerData.ItemIds[playerData.SelectedSlotIndex],
                    out var weapon))
            {
                return;
            }

            if (!CanHit(weapon))
            {
                return;
            }

            var isSurface = ApplyRaycast(connection, ray, weapon, isStrongHit);
            weapon.IsReady = false;
            StartHitCoroutines(weapon);
        }

        private void StartHitCoroutines(MeleeWeaponData meleeWeapon)
        {
            _coroutineRunner.StartCoroutine(Utils.DoActionAfterDelay(() => ResetHit(meleeWeapon), meleeWeapon.TimeBetweenHit));
        }

        private void ResetHit(MeleeWeaponData meleeWeapon)
        {
            if (meleeWeapon is not null)
                meleeWeapon.IsReady = true;
        }

        private bool ApplyRaycast(NetworkConnectionToClient source, Ray ray, MeleeWeaponData meleeWeapon,
            bool isStrongHit)
        {
            var raycastResult = Physics.Raycast(ray, out var rayHit, meleeWeapon.Range, Constants.attackMask);
            if (!raycastResult) return false;
            if (rayHit.collider.CompareTag("Head"))
            {
                HitImpact(source, rayHit, (int) (meleeWeapon.HeadMultiplier * meleeWeapon.DamageToPlayer));
            }

            if (rayHit.collider.CompareTag("Leg"))
            {
                HitImpact(source, rayHit, (int) (meleeWeapon.LegMultiplier * meleeWeapon.DamageToPlayer));
            }

            if (rayHit.collider.CompareTag("Chest"))
            {
                HitImpact(source, rayHit, (int) (meleeWeapon.ChestMultiplier * meleeWeapon.DamageToPlayer));
            }

            if (rayHit.collider.CompareTag("Arm"))
            {
                HitImpact(source, rayHit, (int) (meleeWeapon.ArmMultiplier * meleeWeapon.DamageToPlayer));
            }

            if (rayHit.collider.CompareTag("Chunk"))
            {
                var targetBlock = Vector3Int.FloorToInt(rayHit.point - rayHit.normal / 2);
                if (isStrongHit)
                {
                    var validPositions = _lineExplosionArea.GetExplodedBlocks(3, targetBlock);
                    _server.MapUpdater.DestroyBlocks(validPositions);
                }
                else
                {
                    var validPositions = _lineExplosionArea.GetExplodedBlocks(1, targetBlock);
                    _server.MapUpdater.DestroyBlocks(validPositions);
                }

                return true;
            }

            return false;
        }

        private void HitImpact(NetworkConnectionToClient source, RaycastHit rayHit, int damage)
        {
            var receiver = rayHit.collider.gameObject.GetComponentInParent<NetworkIdentity>().connectionToClient;
            if (source != receiver)
            {
                _server.Damage(source, receiver, damage);
                _particleFactory.CreateBlood(rayHit.point, Quaternion.LookRotation(rayHit.normal));
            }
        }

        private bool CanHit(MeleeWeaponData meleeWeapon)
        {
            return meleeWeapon.IsReady;
        }
    }
}
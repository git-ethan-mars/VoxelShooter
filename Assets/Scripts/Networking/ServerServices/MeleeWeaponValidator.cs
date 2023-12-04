using System.Collections;
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
        private readonly IDamageArea _lineDamageArea;
        private readonly BlockHealthSystem _blockHealthSystem;

        public MeleeWeaponValidator(IServer server, ICoroutineRunner coroutineRunner, IParticleFactory particleFactory,
            BlockHealthSystem blockHealthSystem)
        {
            _server = server;
            _particleFactory = particleFactory;
            _coroutineRunner = coroutineRunner;
            _blockHealthSystem = blockHealthSystem;
            _lineDamageArea = new LineDamageArea(_server.MapProvider);
        }

        public void Hit(NetworkConnectionToClient connection, Ray ray, bool isStrongHit)
        {
            var playerData = _server.Data.GetPlayerData(connection);
            var meleeWeapon = (MeleeWeaponItem) playerData.Items[playerData.SelectedSlotIndex];
            var meleeWeaponData = (MeleeWeaponData) playerData.ItemData[playerData.SelectedSlotIndex];

            if (!CanHit(meleeWeaponData))
            {
                return;
            }

            var isSurface = ApplyRaycast(connection, ray, meleeWeapon, isStrongHit);
            _coroutineRunner.StartCoroutine(ResetHit(connection, meleeWeapon, meleeWeaponData));
        }

        private IEnumerator ResetHit(NetworkConnectionToClient connection, MeleeWeaponItem configure,
            MeleeWeaponData data)
        {
            data.IsReady = false;
            var waitForHitReset = new WaitWithoutSlotChange(_server, connection, configure.timeBetweenHit);
            while (true)
            {
                yield return waitForHitReset;
                if (waitForHitReset.CompletedSuccessfully || waitForHitReset.IsAborted)
                {
                    break;
                }

                waitForHitReset = new WaitWithoutSlotChange(_server, connection, configure.timeBetweenHit);
            }

            if (waitForHitReset.CompletedSuccessfully)
            {
                data.IsReady = true;
            }
        }

        private bool ApplyRaycast(NetworkConnectionToClient source, Ray ray, MeleeWeaponItem meleeWeapon,
            bool isStrongHit)
        {
            var raycastResult = Physics.Raycast(ray, out var rayHit, meleeWeapon.range, Constants.attackMask);
            if (!raycastResult) return false;
            if (rayHit.collider.CompareTag("Head"))
            {
                HitImpact(source, rayHit, (int) (meleeWeapon.headMultiplier * meleeWeapon.damageToPlayer));
            }

            if (rayHit.collider.CompareTag("Leg"))
            {
                HitImpact(source, rayHit, (int) (meleeWeapon.legMultiplier * meleeWeapon.damageToPlayer));
            }

            if (rayHit.collider.CompareTag("Chest"))
            {
                HitImpact(source, rayHit, (int) (meleeWeapon.chestMultiplier * meleeWeapon.damageToPlayer));
            }

            if (rayHit.collider.CompareTag("Arm"))
            {
                HitImpact(source, rayHit, (int) (meleeWeapon.armMultiplier * meleeWeapon.damageToPlayer));
            }

            if (rayHit.collider.CompareTag("Chunk"))
            {
                var targetBlock = Vector3Int.FloorToInt(rayHit.point - rayHit.normal / 2);
                if (isStrongHit)
                {
                    _blockHealthSystem.DamageBlock(targetBlock, 3, meleeWeapon.damageToBlock, _lineDamageArea);
                }
                else
                {
                    _blockHealthSystem.DamageBlock(targetBlock, 1, meleeWeapon.damageToBlock, _lineDamageArea);
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
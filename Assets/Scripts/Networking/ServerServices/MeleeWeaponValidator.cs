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
        private readonly AudioService _audioService;
        private readonly BlockDestructionBehaviour _weakHitDestructionBehaviour;
        private readonly BlockDestructionBehaviour _strongHitDestructionBehaviour;

        public MeleeWeaponValidator(IServer server, CustomNetworkManager networkManager, AudioService audioService)
        {
            _server = server;
            _coroutineRunner = networkManager;
            _particleFactory = networkManager.ParticleFactory;
            _audioService = audioService;
            var weakHitArea = new SingleBlockArea(server.MapProvider);
            _weakHitDestructionBehaviour = new BlockDestructionBehaviour(server, weakHitArea);
            var strongHitArea = new LineBlockArea(server.MapProvider);
            _strongHitDestructionBehaviour = new BlockDestructionBehaviour(server, strongHitArea);
        }

        public void Hit(NetworkConnectionToClient connection, Ray ray, bool isStrongHit)
        {
            var playerData = _server.GetPlayerData(connection);
            var meleeWeapon = (MeleeWeaponItem) playerData.SelectedItem;
            var meleeWeaponData = (MeleeWeaponItemData) playerData.SelectedItemData;

            if (!CanHit(meleeWeaponData))
            {
                return;
            }

            var isSurface = ApplyRaycast(connection, ray, meleeWeapon, isStrongHit);
            _coroutineRunner.StartCoroutine(ResetHit(connection, meleeWeapon, meleeWeaponData));
            _audioService.SendAudio(isSurface ? meleeWeapon.diggingAudio : meleeWeapon.hittingAudio,
                connection.identity);
        }

        private IEnumerator ResetHit(NetworkConnectionToClient connection, MeleeWeaponItem configure,
            MeleeWeaponItemData itemData)
        {
            itemData.IsReady = false;
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
                itemData.IsReady = true;
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
                    _strongHitDestructionBehaviour.DamageBlocks(targetBlock, meleeWeapon.damageToBlock);
                }
                else
                {
                    _weakHitDestructionBehaviour.DamageBlocks(targetBlock, meleeWeapon.damageToBlock);
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
                var blood = _particleFactory.CreateBlood(rayHit.point, Quaternion.LookRotation(rayHit.normal));
                NetworkServer.Spawn(blood);
            }
        }

        private bool CanHit(MeleeWeaponItemData meleeWeaponItem)
        {
            return meleeWeaponItem.IsReady;
        }
    }
}
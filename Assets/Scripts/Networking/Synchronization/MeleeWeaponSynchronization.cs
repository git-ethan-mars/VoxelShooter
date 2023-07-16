using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using Explosions;
using Infrastructure.AssetManagement;
using Infrastructure.Factory;
using Mirror;
using Networking.Messages;
using UnityEngine;

namespace Networking.Synchronization
{
    public class MeleeWeaponSynchronization : NetworkBehaviour
    {
        private IServer _server;
        private IParticleFactory _particleFactory;
        private List<AudioClip> _audioClips;
        private LineExplosionArea _lineExplosionArea;

        public void Construct(IParticleFactory particleFactory, IAssetProvider assets, IServer server)
        {
            _server = server;
            _particleFactory = particleFactory;
            _audioClips = assets.LoadAll<AudioClip>("Audio/Sounds").ToList();
            _lineExplosionArea = new LineExplosionArea(_server.MapProvider);
        }

        [Command]
        public void CmdHit(Ray ray, int weaponId, bool isStrongHit, NetworkConnectionToClient source = null)
        {
            var result = _server.ServerData.TryGetPlayerData(source, out var playerData);
            if (!result || !playerData.IsAlive) return;
            var weapon = playerData.MeleeWeaponsById[weaponId];
            if (!CanHit(weapon)) return;
            var isSurface = ApplyRaycast(source, ray, weapon, isStrongHit);
            weapon.IsReady = false;
            if (isSurface)
            {
                GetComponent<SoundSynchronization>().PlayAudioClip(source!.identity,
                    _audioClips.FindIndex(audioClip => audioClip == weapon.DiggingAudioClip), weapon.DiggingVolume);
            }
            else
            {
                GetComponent<SoundSynchronization>().PlayAudioClip(source!.identity,
                    _audioClips.FindIndex(audioClip => audioClip == weapon.HitAudioClip), weapon.HitVolume);
            }

            StartHitCoroutines(weapon);
        }

        [Server]
        private void StartHitCoroutines(MeleeWeaponData meleeWeapon)
        {
            StartCoroutine(WaitForSeconds(() => ResetHit(meleeWeapon), meleeWeapon.TimeBetweenHit));
        }

        [Server]
        private void ResetHit(MeleeWeaponData meleeWeapon)
        {
            if (meleeWeapon is not null)
                meleeWeapon.IsReady = true;
        }

        [Server]
        private bool ApplyRaycast(NetworkConnectionToClient source, Ray ray, MeleeWeaponData meleeWeapon,
            bool isStrongHit)
        {
            var raycastResult = Physics.Raycast(ray, out var rayHit, meleeWeapon.Range, Constants.AttackMask);
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
                    foreach (var position in validPositions)
                        _server.MapUpdater.SetBlockByGlobalPosition(position, new BlockData());
                    NetworkServer.SendToAll(new UpdateMapMessage(
                        validPositions.ToArray(), new BlockData[validPositions.Count]));
                }
                else
                {
                    var blocks = _lineExplosionArea.GetExplodedBlocks(1, targetBlock);
                    if (blocks.Count > 0)
                    {
                        _server.MapUpdater.SetBlockByGlobalPosition(blocks[0], new BlockData());
                        NetworkServer.SendToAll(new UpdateMapMessage(new[] {targetBlock}, new BlockData[1]));
                    }
                }

                return true;
            }

            return false;
        }

        [Server]
        private void HitImpact(NetworkConnectionToClient source, RaycastHit rayHit, int damage)
        {
            var receiver = rayHit.collider.gameObject.GetComponentInParent<NetworkIdentity>().connectionToClient;
            if (source != receiver)
            {
                GetComponent<HealthSynchronization>().Damage(source, receiver, damage);
                _particleFactory.CreateBlood(rayHit.point, Quaternion.LookRotation(rayHit.normal));
            }
        }

        [Server]
        private bool CanHit(MeleeWeaponData meleeWeapon) => meleeWeapon.IsReady;

        private static IEnumerator WaitForSeconds(Action action, float timeInSeconds)
        {
            yield return new WaitForSeconds(timeInSeconds);
            action();
        }
    }
}
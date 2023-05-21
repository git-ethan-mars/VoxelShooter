using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using Infrastructure.AssetManagement;
using Infrastructure.Factory;
using Mirror;
using Networking.Messages;
using UnityEngine;

namespace Networking.Synchronization
{
    public class MeleeWeaponSynchronization : NetworkBehaviour
    {
        private ServerData _serverData;
        private IParticleFactory _particleFactory;
        private List<AudioClip> _audioClips;

        public void Construct(IParticleFactory particleFactory, IAssetProvider assets, ServerData serverData)
        {
            _serverData = serverData;
            _particleFactory = particleFactory;
            _audioClips = assets.LoadAll<AudioClip>("Audio/Sounds").ToList();
        }

        [Command]
        public void CmdHit(Ray ray, int weaponId, bool isStrongHit, NetworkConnectionToClient source = null)
        {
            var weapon = _serverData.GetPlayerData(source).MeleeWeaponsById[weaponId];
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
            meleeWeapon.IsReady = true;
        }
        
        [Server]
        private bool ApplyRaycast(NetworkConnectionToClient source,Ray ray, MeleeWeaponData meleeWeapon, bool isStrongHit)
        {
            var raycastResult = Physics.Raycast(ray, out var rayHit, meleeWeapon.Range);
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
                    var validPositions = new List<Vector3Int>();
                    for (var i = -1; i <= 1; i++)
                    {
                        if (_serverData.Map.IsValidPosition(targetBlock + new Vector3Int(0, i, 0)))
                        {
                            _serverData.Map.SetBlockByGlobalPosition(targetBlock + new Vector3Int(0, i, 0), new BlockData());
                            validPositions.Add(targetBlock + new Vector3Int(0, i, 0));
                        }
                    }


                    NetworkServer.SendToAll(new UpdateMapMessage(
                        validPositions.ToArray(), new BlockData[validPositions.Count]));
                }
                else
                {
                    if (_serverData.Map.IsValidPosition(targetBlock))
                    {
                        _serverData.Map.SetBlockByGlobalPosition(targetBlock, new BlockData());
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
            if (!receiver.identity.isLocalPlayer)
            {
                GetComponent<HealthSynchronization>().Damage(source, receiver, damage);
                _particleFactory.CreateBlood(rayHit.point);
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
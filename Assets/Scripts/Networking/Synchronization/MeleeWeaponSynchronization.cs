using System;
using System.Collections;
using Data;
using Infrastructure.Factory;
using Mirror;
using PlayerLogic;
using UnityEngine;

namespace Networking.Synchronization
{
    public class MeleeWeaponSynchronization : NetworkBehaviour
    {
        private ServerData _serverData;
        private IParticleFactory _particleFactory;

        public void Construct(IParticleFactory particleFactory, ServerData serverData)
        {
            _serverData = serverData;
            _particleFactory = particleFactory;
        }

        [Command]
        public void CmdHit(Ray ray, int weaponId, bool isSurface, NetworkConnectionToClient conn = null)
        {
            var weapon = _serverData.GetPlayerData(conn!.connectionId).MeleeWeaponsById[weaponId];
            if (!CanHit(weapon)) return;
            ApplyRaycast(ray, weapon);
            Hit(weapon, isSurface);
        }

        [TargetRpc]
        private void SendWeaponState(int weaponId, bool isSurface)
        {
            var weapon = GetComponent<PlayerLogic.Inventory>().MeleeWeapons[weaponId];
            weapon.IsReady = false;
            var audioSource = GetComponent<AudioSource>();
            audioSource.clip = isSurface ? weapon.DiggingAudioClip : weapon.HitAudioClip;
            audioSource.volume = isSurface ? weapon.DiggingVolume : weapon.HitVolume;
            audioSource.Play();
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
            ResetOnClient(meleeWeapon.ID);
        }

        [TargetRpc]
        private void ResetOnClient(int weaponId)
        {
            var weapon = GetComponent<PlayerLogic.Inventory>().MeleeWeapons[weaponId];
            weapon.IsReady = true;
        }

        [Server]
        private void Hit(MeleeWeaponData meleeWeapon, bool isSurface)
        {
            meleeWeapon.IsReady = false;
            SendWeaponState(meleeWeapon.ID, isSurface);
            StartHitCoroutines(meleeWeapon);
        }

        [Server]
        private void ApplyRaycast(Ray ray, MeleeWeaponData meleeWeapon)
        {
            var raycastResult = Physics.Raycast(ray, out var rayHit, meleeWeapon.Range);
            if (!raycastResult) return;
            if (rayHit.collider.CompareTag("Head"))
            {
                HitImpact(rayHit, (int) (meleeWeapon.HeadMultiplier * meleeWeapon.DamageToPlayer));
            }

            if (rayHit.collider.CompareTag("Leg"))
            {
                HitImpact(rayHit, (int) (meleeWeapon.LegMultiplier * meleeWeapon.DamageToPlayer));
            }

            if (rayHit.collider.CompareTag("Chest"))
            {
                HitImpact(rayHit, (int) (meleeWeapon.ChestMultiplier * meleeWeapon.DamageToPlayer));
            }

            if (rayHit.collider.CompareTag("Arm"))
            {
                HitImpact(rayHit, (int) (meleeWeapon.ArmMultiplier * meleeWeapon.DamageToPlayer));
            }
        }

        private void HitImpact(RaycastHit rayHit, int damage)
        {
            var connection = rayHit.collider.gameObject.GetComponentInParent<NetworkIdentity>().connectionToClient;
            GetComponent<HealthSynchronization>().Damage(connection, damage); 
            _particleFactory.CreateBlood(rayHit.point);
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
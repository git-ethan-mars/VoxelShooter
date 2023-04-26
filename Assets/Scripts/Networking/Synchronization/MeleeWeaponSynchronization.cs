using System;
using System.Collections;
using Data;
using Infrastructure.Factory;
using Mirror;
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
        public void CmdHitSingle(Ray ray, int weaponId, NetworkConnectionToClient conn = null)
        {
            var weapon = _serverData.GetPlayerData(conn!.connectionId).MeleeWeaponsById[weaponId];
            if (!CanHit(weapon)) return;
            ApplyRaycast(ray, weapon);
            Hit(weapon);
        }

        [Command]
        public void CmdHitAutomatic(Ray ray, int weaponId, NetworkConnectionToClient conn = null)
        {
            var weapon = _serverData.GetPlayerData(conn!.connectionId).MeleeWeaponsById[weaponId];
            if (!CanHit(weapon)) return;
            ApplyRaycast(ray, weapon);
            Hit(weapon);
        }

        [TargetRpc]
        private void SendWeaponState(int weaponId)
        {
            var weapon = GetComponent<PlayerLogic.Inventory>().MeleeWeapons[weaponId];
            var audioSource = GetComponent<AudioSource>();
            audioSource.clip = weapon.HitAudioClip;
            audioSource.volume = weapon.HitVolume;
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
        }

        [Server]
        private void Hit(MeleeWeaponData meleeWeapon)
        {
            meleeWeapon.IsReady = false;
            SendWeaponState(meleeWeapon.ID);
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
        private bool CanHit(MeleeWeaponData meleeWeapon) => meleeWeapon.IsReady && !meleeWeapon.IsReloading;

        private static IEnumerator WaitForSeconds(Action action, float timeInSeconds)
        {
            yield return new WaitForSeconds(timeInSeconds);
            action();
        }
    }
}
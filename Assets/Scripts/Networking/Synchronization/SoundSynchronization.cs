using Data;
using Infrastructure.Services;
using Mirror;
using UnityEngine;

namespace Networking.Synchronization
{
    public class SoundSynchronization : NetworkBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        private IStaticDataService _staticData;

        private void Awake()
        {
            _staticData = AllServices.Container.Single<IStaticDataService>();
        }

        public void Start()
        {
            audioSource = GetComponent<AudioSource>();
            
        }
        [TargetRpc]
        public void PlayAudioClip(int weaponId, SoundType soundType)
        {
            if (soundType == SoundType.Shooting)
            {
                PlayShoot(weaponId);
            }

            if (soundType == SoundType.Reloading)
            {
                PlayReload(weaponId);
            }
        }

        private void PlayReload(int weaponId)
        {
            var weapon = (RangeWeaponItem)_staticData.GetItem(weaponId);
            PlayAudioClipInternal(weapon.reloadingAudioClip, weapon.reloadingVolume);
        }

        private void PlayShoot(int weaponId)
        {
            var weapon = (RangeWeaponItem)_staticData.GetItem(weaponId);
            PlayAudioClipInternal(weapon.shootingAudioClip, weapon.shootingVolume);
        }

        private void PlayAudioClipInternal(AudioClip audioClip, float volume)
        {
            audioSource.clip = audioClip;
            audioSource.volume = volume;
            audioSource.Play();
        }
    }
}
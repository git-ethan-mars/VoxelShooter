using System.Collections.Generic;
using System.Linq;
using Data;
using Infrastructure.AssetManagement;
using Infrastructure.Services;
using Mirror;
using UnityEngine;

namespace Networking.Synchronization
{
    public class SoundSynchronization : NetworkBehaviour
    {
        private IAssetProvider _assets;
        private List<AudioClip> _audioClips;

        private void Awake()
        {
            _assets = AllServices.Container.Single<IAssetProvider>();
            _audioClips = _assets.LoadAll<AudioClip>("Audio/Sounds").ToList();
        }

        [ClientRpc]
        public void PlayAudioClip(NetworkIdentity source, int audioClipId, float volume)
        {
            var audioSource = source.GetComponent<AudioSource>();
            audioSource.clip = _audioClips[audioClipId];
            audioSource.volume = volume;
            audioSource.Play();
        }
    }
}
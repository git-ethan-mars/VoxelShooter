using Infrastructure;
using Infrastructure.Services.StaticData;
using Infrastructure.Services.Storage;
using Networking.ClientServices;
using Networking.Messages.Responses;
using UI.SettingsMenu;
using UnityEngine;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class SurroundingSoundHandler : ResponseHandler<SurroundingSoundResponse>
    {
        private const float Sound3D = 1.0f;

        public float SoundMultiplier { get; set; }

        private readonly IStaticDataService _staticData;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly AudioPool _audioPool;

        public SurroundingSoundHandler(CustomNetworkManager networkManager, AudioPool audioPool)
        {
            _staticData = networkManager.StaticData;
            _coroutineRunner = networkManager;
            _audioPool = audioPool;
            SoundMultiplier = networkManager.StorageService.Load<VolumeSettingsData>(Constants.VolumeSettingsKey).SoundVolume;
        }

        protected override void OnResponseReceived(SurroundingSoundResponse response)
        {
            var audioSource = _audioPool.Get();
            var audio = _staticData.GetAudio(response.SoundId);
            audioSource.transform.position = response.Position;
            audioSource.clip = audio.clip;
            audioSource.volume = audio.volume * SoundMultiplier;
            audioSource.minDistance = audio.minDistance;
            audioSource.maxDistance = audio.maxDistance;
            audioSource.spatialBlend = Sound3D;
            audioSource.Play();
            _coroutineRunner.StartCoroutine(_audioPool.ReleaseOnDelay(audioSource, audio.clip.length));
        }
    }
}
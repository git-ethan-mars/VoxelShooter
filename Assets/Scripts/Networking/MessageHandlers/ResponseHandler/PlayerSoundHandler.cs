using Infrastructure;
using Infrastructure.Services.StaticData;
using Infrastructure.Services.Storage;
using Mirror;
using Networking.ClientServices;
using Networking.Messages.Responses;
using UI.SettingsMenu;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class PlayerSoundHandler : ResponseHandler<PlayerSoundResponse>
    {
        private const float Sound3D = 1.0f;
        private const float Sound2D = 0.0f;
        public float SoundMultiplier { get; set; }

        private readonly IStaticDataService _staticData;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly AudioPool _audioPool;


        public PlayerSoundHandler(CustomNetworkManager customNetworkManager, AudioPool audioPool)
        {
            _staticData = customNetworkManager.StaticData;
            _coroutineRunner = customNetworkManager;
            _audioPool = audioPool;
            SoundMultiplier = customNetworkManager.StorageService.Load<VolumeSettingsData>(Constants.VolumeSettingsKey)
                .SoundVolume;
        }

        protected override void OnResponseReceived(PlayerSoundResponse response)
        {
            if (response.Source != null)
            {
                var audioSource = _audioPool.Get();
                var transformFollower = audioSource.GetComponent<TransformFollower>();
                transformFollower.Target = response.Source.transform;
                transformFollower.enabled = true;
                var audio = _staticData.GetAudio(response.SoundId);
                audioSource.clip = audio.clip;
                audioSource.volume = audio.volume * SoundMultiplier;
                audioSource.minDistance = audio.minDistance;
                audioSource.maxDistance = audio.maxDistance;
                audioSource.spatialBlend = response.Source == NetworkClient.localPlayer ? Sound2D : Sound3D;
                audioSource.Play();
                _coroutineRunner.StartCoroutine(_audioPool.ReleaseOnDelay(audioSource, audio.clip.length));
            }
        }
    }
}
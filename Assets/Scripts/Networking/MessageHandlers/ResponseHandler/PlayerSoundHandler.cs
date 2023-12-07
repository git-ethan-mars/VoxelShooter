using Infrastructure;
using Infrastructure.Services.StaticData;
using Mirror;
using Networking.ClientServices;
using Networking.Messages.Responses;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class PlayerSoundHandler : ResponseHandler<PlayerSoundResponse>
    {
        private const float Sound3D = 1.0f;
        private const float Sound2D = 0.0f;

        private readonly IStaticDataService _staticData;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly AudioPool _audioPool;

        public PlayerSoundHandler(IStaticDataService staticData, ICoroutineRunner coroutineRunner, AudioPool audioPool)
        {
            _staticData = staticData;
            _coroutineRunner = coroutineRunner;
            _audioPool = audioPool;
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
                audioSource.volume = audio.volume;
                audioSource.minDistance = audio.minDistance;
                audioSource.maxDistance = audio.maxDistance;
                audioSource.spatialBlend = response.Source == NetworkClient.localPlayer ? Sound2D : Sound3D;
                audioSource.Play();
                _coroutineRunner.StartCoroutine(_audioPool.ReleaseOnDelay(audioSource, audio.clip.length));
            }
        }
    }
}
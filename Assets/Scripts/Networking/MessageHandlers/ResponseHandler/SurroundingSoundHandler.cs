using Infrastructure;
using Infrastructure.Services.StaticData;
using Networking.ClientServices;
using Networking.Messages.Responses;
using UnityEngine;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class SurroundingSoundHandler : ResponseHandler<SurroundingSoundResponse>
    {
        private const float Sound3D = 1.0f;

        private readonly IStaticDataService _staticData;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly AudioPool _audioPool;

        public SurroundingSoundHandler(IStaticDataService staticData, ICoroutineRunner coroutineRunner,
            AudioPool audioPool)
        {
            _staticData = staticData;
            _coroutineRunner = coroutineRunner;
            _audioPool = audioPool;
        }

        protected override void OnResponseReceived(SurroundingSoundResponse response)
        {
            var audioSource = _audioPool.Get();
            var audio = _staticData.GetAudio(response.SoundId);
            audioSource.transform.position = response.Position;
            audioSource.clip = audio.clip;
            audioSource.volume = audio.volume;
            audioSource.minDistance = audio.minDistance;
            audioSource.maxDistance = audio.maxDistance;
            audioSource.spatialBlend = Sound3D;
            audioSource.Play();
            _coroutineRunner.StartCoroutine(_audioPool.ReleaseOnDelay(audioSource, audio.clip.length));
        }
    }
}
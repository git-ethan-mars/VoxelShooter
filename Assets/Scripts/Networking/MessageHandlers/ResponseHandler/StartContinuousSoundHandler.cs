using Infrastructure.Services.StaticData;
using Mirror;
using Networking.Messages.Responses;
using PlayerLogic;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class StartContinuousSoundHandler : ResponseHandler<StartContinuousSoundResponse>
    {
        private const float Sound3D = 1.0f;
        private readonly IStaticDataService _staticData;

        public StartContinuousSoundHandler(IStaticDataService staticData)
        {
            _staticData = staticData;
        }

        protected override void OnResponseReceived(StartContinuousSoundResponse response)
        {
            if (response.Source != null)
            {
                var audio = _staticData.GetAudio(response.SoundId);
                var audioSource = response.Source.GetComponent<Player>().ContinuousAudio;
                audioSource.clip = audio.clip;
                audioSource.volume = audio.volume;
                audioSource.minDistance = audio.minDistance;
                audioSource.maxDistance = audio.maxDistance;
                audioSource.loop = true;
                if (response.Source != NetworkClient.localPlayer)
                {
                    audioSource.spatialBlend = Sound3D;
                }

                audioSource.Play();
            }
        }
    }
}
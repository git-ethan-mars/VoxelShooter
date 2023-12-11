using Infrastructure.Services.StaticData;
using Mirror;
using Networking.Messages.Responses;
using PlayerLogic;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class StartContinuousSoundHandler : ResponseHandler<StartContinuousSoundResponse>
    {
        private const float Sound3D = 1.0f;
        private const float Sound2D = 0.0f;
        
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
                var spatialBlend = Sound2D;
                if (response.Source != NetworkClient.localPlayer)
                {
                    spatialBlend = Sound3D;
                }

                response.Source.GetComponent<Player>().Audio.ChangeContinuousAudio(audio, spatialBlend);
            }
        }
    }
}
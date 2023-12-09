using Networking.Messages.Responses;
using PlayerLogic;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class StopContinuousSoundHandler : ResponseHandler<StopContinuousSoundResponse>
    {
        protected override void OnResponseReceived(StopContinuousSoundResponse response)
        {
            if (response.Source != null)
            {
                var audioSource = response.Source.GetComponent<Player>().ContinuousAudio;
                audioSource.loop = false;
            }
        }
    }
}
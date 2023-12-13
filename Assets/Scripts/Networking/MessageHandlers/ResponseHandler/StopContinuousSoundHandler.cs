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
                response.Source.GetComponent<Player>().Audio.StopContinuousSound();
            }
        }
    }
}
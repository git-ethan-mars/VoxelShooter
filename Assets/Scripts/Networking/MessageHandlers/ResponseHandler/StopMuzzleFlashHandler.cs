using Networking.Messages.Responses;
using UnityEngine;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class StopMuzzleFlashHandler : ResponseHandler<StopMuzzleFlashResponse>
    {
        public StopMuzzleFlashHandler()
        {
            
        }

        protected override void OnResponseReceived(StopMuzzleFlashResponse response)
        {
            if (response.Source != null)
            {
                var particleSystem = response.Source.gameObject.GetComponentInChildren<ParticleSystem>();
                particleSystem.Stop();
            }
        }
    }
}
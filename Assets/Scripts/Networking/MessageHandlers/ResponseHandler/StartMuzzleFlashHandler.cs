using Networking.Messages.Responses;
using UnityEngine;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class StartMuzzleFlashHandler : ResponseHandler<StartMuzzleFlashResponse>
    {
        public StartMuzzleFlashHandler()
        {
            
        }

        protected override void OnResponseReceived(StartMuzzleFlashResponse response)
        {
            if (response.Source != null)
            {
                var particleSystem = response.Source.gameObject.GetComponentInChildren<ParticleSystem>();
                particleSystem.Play();
            }
        }
    }
}
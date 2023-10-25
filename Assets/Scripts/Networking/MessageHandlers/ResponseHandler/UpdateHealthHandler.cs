using System;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class HealthHandler : ResponseHandler<HealthResponse>
    {
        public event Action<int> HealthChanged;

        protected override void OnResponseReceived(HealthResponse response)
        {
            HealthChanged?.Invoke(response.Health);
        }
    }
}
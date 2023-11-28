using Mirror;
using Networking.Messages.Responses;
using PlayerLogic;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class HealthHandler : ResponseHandler<HealthResponse>
    {
        protected override void OnResponseReceived(HealthResponse response)
        {
            if (NetworkClient.connection.identity != null &&
                NetworkClient.connection.identity.TryGetComponent<Player>(out var player))
            {
                player.Health.Value = response.Health;
            }
        }
    }
}
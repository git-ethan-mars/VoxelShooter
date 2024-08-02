using Mirror;
using Networking.Messages.Responses;
using PlayerLogic;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class BlockUseHandler : ResponseHandler<BlockUseResponse>
    {
        protected override void OnResponseReceived(BlockUseResponse response)
        {
            if (NetworkClient.connection.identity != null &&
                NetworkClient.connection.identity.TryGetComponent<Player>(out var player))
            {
                player.BlockCount.Value = response.Count;
            }
        }
    }
}
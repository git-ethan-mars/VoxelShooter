using Networking.Messages.Responses;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class FallBlockHandler : ResponseHandler<FallBlockResponse>
    {
        private readonly Client _client;

        public FallBlockHandler(Client client)
        {
            _client = client;
        }

        protected override void OnResponseReceived(FallBlockResponse message)
        {
            _client.FallMeshGenerator.GenerateFallBlocks(message.Positions, message.Colors);
        }
    }
}
using Networking.Messages.Responses;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class FallBlockHandler : ResponseHandler<FallBlockResponse>
    {
        private readonly IClient _client;

        public FallBlockHandler(IClient client)
        {
            _client = client;
        }

        protected override void OnResponseReceived(FallBlockResponse message)
        {
            _client.FallMeshGenerator.GenerateFallBlocks(message.Positions, message.Colors);
        }
    }
}
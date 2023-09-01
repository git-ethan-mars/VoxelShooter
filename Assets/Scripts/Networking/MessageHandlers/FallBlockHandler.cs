using Networking.Messages.Responses;

namespace Networking
{
    public class FallBlockHandler : MessageHandler<FallBlockResponse>
    {
        private readonly Client _client;

        public FallBlockHandler(Client client)
        {
            _client = client;
        }

        protected override void OnMessageReceived(FallBlockResponse message)
        {
            _client.FallMeshGenerator.GenerateFallBlocks(message.Positions, message.Colors);
        }
    }
}
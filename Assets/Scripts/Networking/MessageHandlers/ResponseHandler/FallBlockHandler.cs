using Generators;
using Networking.Messages.Responses;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class FallBlockHandler : ResponseHandler<FallBlockResponse>
    {
        private readonly FallMeshGenerator _fallMeshGenerator;

        public FallBlockHandler(CustomNetworkManager networkManager)
        { 
            _fallMeshGenerator = new FallMeshGenerator(networkManager);
        }

        protected override void OnResponseReceived(FallBlockResponse message)
        {
            _fallMeshGenerator.GenerateFallBlocks(message.Blocks);
        }
    }
}
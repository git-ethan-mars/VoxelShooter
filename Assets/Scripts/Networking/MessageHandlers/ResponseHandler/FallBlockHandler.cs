using Generators;
using Networking.Messages.Responses;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class FallBlockHandler : ResponseHandler<FallBlockResponse>
    {
        private readonly FallMeshGenerator _fallMeshGenerator;

        public FallBlockHandler(FallMeshGenerator fallMeshGenerator)
        {
            _fallMeshGenerator = fallMeshGenerator;
        }

        protected override void OnResponseReceived(FallBlockResponse message)
        {
            _fallMeshGenerator.GenerateFallBlocks(message.Blocks);
        }
    }
}
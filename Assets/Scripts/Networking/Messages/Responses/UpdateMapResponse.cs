using Data;
using Mirror;

namespace Networking.Messages.Responses
{
    public struct UpdateMapResponse : NetworkMessage
    {
        public readonly BlockDataWithPosition[] Blocks;

        public UpdateMapResponse(BlockDataWithPosition[] blocks)
        {
            Blocks = blocks;
        }
    }
}
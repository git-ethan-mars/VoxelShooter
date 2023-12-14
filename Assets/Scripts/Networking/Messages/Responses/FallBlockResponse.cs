using Data;
using Mirror;

namespace Networking.Messages.Responses
{
    public struct FallBlockResponse : NetworkMessage
    {
        public readonly BlockDataWithPosition[] Blocks;

        public FallBlockResponse(BlockDataWithPosition[] blocks)
        {
            Blocks = blocks;
        }
    }
}
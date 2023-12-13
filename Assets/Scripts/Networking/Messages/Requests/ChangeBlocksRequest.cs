using Data;
using Mirror;

namespace Networking.Messages.Requests
{
    public struct AddBlocksRequest : NetworkMessage
    {
        public readonly BlockDataWithPosition[] Blocks;

        public AddBlocksRequest(BlockDataWithPosition[] blocks)
        {
            Blocks = blocks;
        }
    }
}
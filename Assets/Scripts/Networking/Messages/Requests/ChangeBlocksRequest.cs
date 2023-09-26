using Data;
using Mirror;
using UnityEngine;

namespace Networking.Messages.Requests
{
    public struct AddBlocksRequest : NetworkMessage
    {
        public readonly Vector3Int[] GlobalPositions;
        public readonly BlockData[] Blocks;
        public readonly int ItemId;

        public AddBlocksRequest(Vector3Int[] globalPositions, BlockData[] blocks, int itemId)
        {
            GlobalPositions = globalPositions;
            Blocks = blocks;
            ItemId = itemId;
        }
    }
}
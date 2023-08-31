using Data;
using Mirror;
using UnityEngine;

namespace Networking.Messages.Responses
{
    public struct FallBlockResponse : NetworkMessage
    {
        public readonly Vector3Int[] Positions;
        public readonly BlockData[] BlockData;
        public uint ComponentId;

        public FallBlockResponse(Vector3Int[] positions, BlockData[] blockData, uint componentId)
        {
            Positions = positions;
            BlockData = blockData;
            ComponentId = componentId;
        }
    }
}
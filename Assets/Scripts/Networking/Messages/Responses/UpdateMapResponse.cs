using Data;
using Mirror;
using UnityEngine;

namespace Networking.Messages.Responses
{
    public struct UpdateMapResponse : NetworkMessage
    {
        public readonly Vector3Int[] Positions;
        public readonly BlockData[] BlockData;

        public UpdateMapResponse(Vector3Int[] positions, BlockData[] blockData)
        {
            Positions = positions;
            BlockData = blockData;
        }
    }
}
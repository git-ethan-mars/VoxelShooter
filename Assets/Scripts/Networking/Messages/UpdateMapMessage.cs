using Data;
using Mirror;
using UnityEngine;

namespace Networking.Messages
{
    public struct UpdateMapMessage : NetworkMessage
    {
        public readonly Vector3Int[] Positions;
        public readonly BlockData[] BlockData;

        public UpdateMapMessage(Vector3Int[] positions, BlockData[] blockData)
        {
            Positions = positions;
            BlockData = blockData;
        }
    }
}
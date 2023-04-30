using Mirror;
using UnityEngine;

namespace Networking.Messages
{
    public struct RemoveBlocksRequest : NetworkMessage
    {
        public readonly Vector3Int[] GlobalPositions;
        public RemoveBlocksRequest(Vector3Int[] globalPositions)
        {
            GlobalPositions = globalPositions;
        }
    }
}
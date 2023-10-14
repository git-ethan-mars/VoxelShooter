using Mirror;
using UnityEngine;

namespace Networking.Messages.Responses
{
    public struct FallBlockResponse : NetworkMessage
    {
        public readonly Vector3Int[] Positions;
        public readonly Color32[] Colors;

        public FallBlockResponse(Vector3Int[] positions, Color32[] colors)
        {
            Positions = positions;
            Colors = colors;
        }
    }
}
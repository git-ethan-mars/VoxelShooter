using Mirror;
using UnityEngine;

namespace Networking.Messages
{
    public struct TntSpawnRequest : NetworkMessage
    {
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly float DelayInSecond;
        public readonly int Radius;

        public TntSpawnRequest(Vector3 position, Quaternion rotation, float delayInSecond, int radius)
        {
            Position = position;
            Rotation = rotation;
            DelayInSecond = delayInSecond;
            Radius = radius;
        }
    }
}
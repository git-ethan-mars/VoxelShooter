using Mirror;
using UnityEngine;

namespace Networking.Messages.Requests
{
    public struct TntSpawnRequest : NetworkMessage
    {
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly Vector3Int ExplosionCenter;

        public TntSpawnRequest(Vector3 position, Quaternion rotation, Vector3Int explosionCenter)
        {
            Position = position;
            Rotation = rotation;
            ExplosionCenter = explosionCenter;
        }
    }
}
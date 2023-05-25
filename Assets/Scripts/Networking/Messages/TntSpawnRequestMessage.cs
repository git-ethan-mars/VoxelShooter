using Mirror;
using UnityEngine;

namespace Networking.Messages
{
    public struct TntSpawnRequest : NetworkMessage
    {
        public readonly int ItemId;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly Vector3Int ExplosionCenter;

        public TntSpawnRequest(int id, Vector3 position, Quaternion rotation, Vector3Int explosionCenter)
        {
            ItemId = id;
            Position = position;
            Rotation = rotation;
            ExplosionCenter = explosionCenter;
        }
    }
}
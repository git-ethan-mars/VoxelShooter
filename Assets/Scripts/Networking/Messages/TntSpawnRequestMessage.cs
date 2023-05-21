using Mirror;
using UnityEngine;

namespace Networking.Messages
{
    public struct TntSpawnRequest : NetworkMessage
    {
        public readonly int ItemId;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly float DelayInSecond;
        public readonly Vector3Int ExplosionCenter;
        public readonly int Radius;
        public readonly int Damage;

        public TntSpawnRequest(int id, Vector3 position, Quaternion rotation, float delayInSecond, Vector3Int explosionCenter, int radius, int damage)
        {
            ItemId = id;
            Position = position;
            Rotation = rotation;
            DelayInSecond = delayInSecond;
            ExplosionCenter = explosionCenter;
            Radius = radius;
            Damage = damage;
        }
    }
}
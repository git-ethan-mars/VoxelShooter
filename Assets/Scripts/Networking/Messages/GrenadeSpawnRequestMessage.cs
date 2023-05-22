using Mirror;
using UnityEngine;

namespace Networking.Messages
{
    public struct GrenadeSpawnRequest : NetworkMessage
    {
        public string GrenadePath;
        public readonly int ItemId;
        public readonly Vector3 Direction;
        public readonly Vector3 Target;
        public readonly float DelayInSecond;
        public readonly int Radius;
        public readonly int Damage;
        public readonly int ThrowForce;

        public GrenadeSpawnRequest(int id, Vector3 direction, Vector3 target, float delayInSecond, int radius, int damage, int throwForce)
        {
            GrenadePath = "Prefabs/SpawningGrenade";
            ItemId = id;
            DelayInSecond = delayInSecond;
            Direction = direction;
            Target = target;
            Radius = radius;
            Damage = damage;
            ThrowForce = throwForce;
        }
    }
}
using Mirror;
using UnityEngine;

namespace Networking.Messages.Requests
{
    public struct GrenadeSpawnRequest : NetworkMessage
    {
        public readonly int ItemId;
        public readonly Ray Ray;
        public readonly float ThrowForce;

        public GrenadeSpawnRequest(int id, Ray ray, float throwForce)
        {
            ItemId = id;
            ThrowForce = throwForce;
            Ray = ray;
        }
    }
}
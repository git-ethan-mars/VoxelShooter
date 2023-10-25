using Mirror;
using UnityEngine;

namespace Networking.Messages.Requests
{
    public struct HitRequest : NetworkMessage
    {
        public readonly Ray Ray;
        public readonly bool IsStrongHit;

        public HitRequest(Ray ray, bool isStrongHit)
        {
            Ray = ray;
            IsStrongHit = isStrongHit;
        }
    }
}
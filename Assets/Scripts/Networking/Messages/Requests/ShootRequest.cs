using Mirror;
using UnityEngine;

namespace Networking.Messages.Requests
{
    public struct ShootRequest : NetworkMessage
    {
        public readonly Ray Ray;
        public readonly bool IsButtonHolding;
        public ShootRequest(Ray ray, bool isButtonHolding)
        {
            Ray = ray;
            IsButtonHolding = isButtonHolding;
        }
    }
}
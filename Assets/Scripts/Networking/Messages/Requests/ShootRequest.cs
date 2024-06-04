using System;
using Mirror;
using UnityEngine;

namespace Networking.Messages.Requests
{
    public struct ShootRequest : NetworkMessage
    {
        public readonly Ray Ray;
        public readonly int Tick;
        public readonly bool IsButtonHolding;
        public ShootRequest(Ray ray, bool isButtonHolding)
        {
            Ray = ray;
            Tick = (int) Math.Floor(NetworkTime.time * NetworkServer.tickRate);
            IsButtonHolding = isButtonHolding;
        }
    }
}
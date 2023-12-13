using Mirror;
using UnityEngine;

namespace Networking.Messages.Requests
{
    public struct RocketSpawnRequest : NetworkMessage
    {
        public readonly Ray Ray;

        public RocketSpawnRequest(Ray ray)
        {
            Ray = ray;
        }
    }
}
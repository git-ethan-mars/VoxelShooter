using Mirror;
using UnityEngine;

namespace Networking.Messages.Requests
{
    public struct TntSpawnRequest : NetworkMessage
    {
        public readonly Ray Ray;

        public TntSpawnRequest(Ray ray)
        {
            Ray = ray;
        }
    }
}
using Mirror;
using UnityEngine;

namespace Networking.Messages.Requests
{
    public struct RocketSpawnRequest : NetworkMessage
    {
        public readonly int ItemId;
        public readonly Ray Ray;

        public RocketSpawnRequest(int id, Ray ray)
        {
            ItemId = id;
            Ray = ray;
        }
    }
}
using Mirror;
using UnityEngine;

namespace Networking.Messages.Requests
{
    public struct DrillSpawnRequest : NetworkMessage
    {
        public readonly int ItemId;
        public readonly Ray Ray;

        public DrillSpawnRequest(int id, Ray ray)
        {
            ItemId = id;
            Ray = ray;
        }
    }
}
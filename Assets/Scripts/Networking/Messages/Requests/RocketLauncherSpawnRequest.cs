using Mirror;
using UnityEngine;

namespace Networking.Messages.Requests
{
    public struct RocketLauncherSpawnRequest : NetworkMessage
    {
        public readonly int ItemId;
        public readonly Ray Ray;

        public RocketLauncherSpawnRequest(int id, Ray ray)
        {
            ItemId = id;
            Ray = ray;
        }
    }
}
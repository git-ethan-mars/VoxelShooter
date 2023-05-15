using Mirror;
using UnityEngine;

namespace Networking.Messages
{
    public struct SpectatorTargetResult : NetworkMessage
    {
        public readonly NetworkIdentity NewTarget;
        public readonly Vector3 Position;

        public SpectatorTargetResult(NetworkIdentity newTarget, Vector3 position)
        {
            NewTarget = newTarget;
            Position = position;
        }
    }
}
using Mirror;
using UnityEngine;

namespace Networking.Messages.Responses
{
    public struct SoundResponse : NetworkMessage
    {
        public readonly int SoundId;
        public readonly Vector3 Position;

        public SoundResponse(int soundId, Vector3 position)
        {
            SoundId = soundId;
            Position = position;
        }
    }
}
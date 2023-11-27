using Mirror;

namespace Networking.Messages.Responses
{
    public struct StopContinuousSoundResponse : NetworkMessage
    {
        public readonly int SoundId;
        public readonly NetworkIdentity Source;

        public StopContinuousSoundResponse(int soundId, NetworkIdentity source)
        {
            SoundId = soundId;
            Source = source;
        }
    }
}
using Mirror;

namespace Networking.Messages.Responses
{
    public struct StopMuzzleFlashResponse : NetworkMessage
    {
        public readonly NetworkIdentity Source;

        public StopMuzzleFlashResponse(NetworkIdentity source)
        {
            Source = source;
        }
    }
}
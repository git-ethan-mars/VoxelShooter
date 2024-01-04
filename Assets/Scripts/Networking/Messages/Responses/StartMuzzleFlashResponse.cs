using Mirror;

namespace Networking.Messages.Responses
{
    public struct StartMuzzleFlashResponse : NetworkMessage
    {
        public readonly NetworkIdentity Source;

        public StartMuzzleFlashResponse(NetworkIdentity source)
        {
            Source = source;
        }
    }
}
using Mirror;

namespace Networking.Messages.Responses
{
    public struct StopContinuousSoundResponse : IMirrorResponse
    {
        public readonly NetworkIdentity Source;

        public StopContinuousSoundResponse(NetworkIdentity source)
        {
            Source = source;
        }
    }
}
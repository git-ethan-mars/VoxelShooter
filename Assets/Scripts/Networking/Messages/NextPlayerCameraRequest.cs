using Mirror;

namespace Networking.Messages
{
    public struct NextPlayerCameraRequest : NetworkMessage
    {
        public int ConnectionId;

        public NextPlayerCameraRequest(int currentTarget)
        {
            ConnectionId = currentTarget;
        }
    }
}
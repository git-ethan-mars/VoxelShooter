using Mirror;

namespace Networking.Messages.Requests
{
    public struct ChangeSlotRequest : NetworkMessage
    {
        public readonly int Index;

        public ChangeSlotRequest(int index)
        {
            Index = index;
        }
    }
}
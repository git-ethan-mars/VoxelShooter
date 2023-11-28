using Mirror;

namespace Networking.Messages.Responses
{
    public struct ChangeSlotResponse : NetworkMessage
    {
        public readonly int Index;

        public ChangeSlotResponse(int index)
        {
            Index = index;
        }
    }
}
using Mirror;

namespace Networking.Messages.Responses
{
    public struct ChangeSlotResultResponse : NetworkMessage
    {
        public readonly int Index;

        public ChangeSlotResultResponse(int index)
        {
            Index = index;
        }
    }
}
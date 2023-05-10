using Mirror;

namespace Networking.Messages
{
    public struct ChangeSlotResult : NetworkMessage
    {
        public readonly int Index;

        public ChangeSlotResult(int index)
        {
            Index = index;
        }
    }
}
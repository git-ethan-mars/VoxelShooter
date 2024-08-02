using Mirror;

namespace Networking.Messages.Responses
{
    public struct BlockUseResponse : NetworkMessage
    {
        public readonly int Count;
        
        public BlockUseResponse(int count)
        {
            Count = count;
        }
    }
}
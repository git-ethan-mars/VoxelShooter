using Mirror;

namespace Networking
{
    public struct MapMessage : NetworkMessage
    {
        public byte[] ByteChunk;
        public bool IsFinalChunk;

        public MapMessage(byte[] byteChunk, bool isFinalChunk)
        {
            ByteChunk = byteChunk;
            IsFinalChunk = isFinalChunk;
        }
    }
}
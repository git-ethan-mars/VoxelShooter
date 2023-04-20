using Mirror;

namespace Networking.Messages
{
    public struct DownloadMapMessage : NetworkMessage
    {
        public readonly byte[] ByteChunk;
        public readonly bool IsFinalChunk;

        public DownloadMapMessage(byte[] byteChunk, bool isFinalChunk)
        {
            ByteChunk = byteChunk;
            IsFinalChunk = isFinalChunk;
        }
    }
}
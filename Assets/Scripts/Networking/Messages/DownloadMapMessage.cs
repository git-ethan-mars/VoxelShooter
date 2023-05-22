using Mirror;

namespace Networking.Messages
{
    public struct DownloadMapMessage : NetworkMessage
    {
        public readonly byte[] ByteChunk;
        public readonly float Progress;

        public DownloadMapMessage(byte[] byteChunk, float progress)
        {
            ByteChunk = byteChunk;
            Progress = progress;
        }
    }
}
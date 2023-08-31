using Mirror;

namespace Networking.Messages.Responses
{
    public struct DownloadMapResponse : NetworkMessage
    {
        public readonly byte[] ByteChunk;
        public readonly float Progress;

        public DownloadMapResponse(byte[] byteChunk, float progress)
        {
            ByteChunk = byteChunk;
            Progress = progress;
        }
    }
}
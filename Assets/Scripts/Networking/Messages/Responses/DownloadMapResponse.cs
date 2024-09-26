namespace Networking.Messages.Responses
{
	public struct DownloadMapResponse : IMirrorResponse
	{
		public readonly byte[] ByteChunk;
		public readonly int StartByte;
		public readonly int TotalBytes;

		public DownloadMapResponse(byte[] byteChunk, int totalBytes, int startByte)
		{
			ByteChunk = byteChunk;
			TotalBytes = totalBytes;
			StartByte = startByte;
		}
	}
}
namespace Networking.Messages
{
	public struct MapDownloadResponse : IResponse
	{
		public readonly byte[] ByteChunk;
		public readonly int Offset;
		public readonly int TotalBytes;

		public MapDownloadResponse(byte[] byteChunk, int offset, int totalBytes)
		{
			ByteChunk = byteChunk;
			Offset = offset;
			TotalBytes = totalBytes;
		}
	}
}
using System;
using System.IO;

namespace VoxelMap
{
	public class MapData
	{
		public int ChunkCount => _chunks.Length;
		public readonly int Width;
		public readonly int Depth;
		public readonly int Height;
		private readonly ChunkData[] _chunks;

		public MapData(ChunkData[] chunks, int width, int height, int depth)
		{
			_chunks = chunks;
			Width = width;
			Height = height;
			Depth = depth;
		}

		public int GetChunkNumberByGlobalPosition(int x, int y, int z)
		{
			return z / ChunkData.ChunkSize +
			       y / ChunkData.ChunkSize * (Depth / ChunkData.ChunkSize) +
			       x / ChunkData.ChunkSize *
			       (Height / ChunkData.ChunkSize * Depth / ChunkData.ChunkSize);
		}

		public ChunkData GetChunkDataByIndex(int chunkIndex)
		{
			AssertChunkIndex(chunkIndex);
			return _chunks[chunkIndex];
		}

		public ChunkData GetChunkByGlobalPosition(int x, int y, int z)
		{
			var chunkIndex = GetChunkNumberByGlobalPosition(x, y, z);
			AssertChunkIndex(chunkIndex);
			return _chunks[chunkIndex];
		}

		public byte[] Serialize()
		{
			using var memoryStream = new MemoryStream();
			MapWriter.WriteMap(this, memoryStream);
			var bytes = memoryStream.ToArray();
			return bytes;
		}

		private void AssertChunkIndex(int chunkIndex)
		{
			if (chunkIndex < 0 || chunkIndex > _chunks.Length)
			{
				throw new ArgumentException($"Invalid chunk index: {chunkIndex}");
			}
		}
	}
}
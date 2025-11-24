using System;
namespace VoxelMap
{
	public readonly struct ChunkData
	{
		public readonly IntPtr VerticesArray;
		public readonly int VerticesCount;
		public readonly IntPtr IndicesArray;
		public readonly int IndicesCount;
		public readonly int Index;

		public ChunkData(IntPtr verticesArray, int verticesCount, IntPtr indicesArray, int indicesCount, int index)
		{
			VerticesArray = verticesArray;
			VerticesCount = verticesCount;
			IndicesArray = indicesArray;
			IndicesCount = indicesCount;
			Index = index;
		}
	}
}
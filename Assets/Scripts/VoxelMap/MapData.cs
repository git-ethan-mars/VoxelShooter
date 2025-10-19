using System;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
namespace VoxelMap
{
	public struct MapData : IDisposable
	{
		public int ChunkCount => Width * Depth * Height / Chunk.ChunkSizeCubed;
		public readonly int Width;
		public readonly int Depth;
		public readonly int Height;
		[NativeDisableContainerSafetyRestriction]
		internal NativeArray<Face> Faces;
		[NativeDisableContainerSafetyRestriction]
		internal NativeArray<VoxelData> Voxels;

		public MapData(NativeArray<VoxelData> voxels, int width, int height, int depth)
		{
			Width = width;
			Height = height;
			Depth = depth;
			Faces = new NativeArray<Face>(voxels.Length, Allocator.Persistent);
			Voxels = voxels;
		}

		public VoxelData this[int x, int y, int z]
		{
			get
			{
				if (!IsValidPosition(x, y, z))
				{
					throw new IndexOutOfRangeException($"{x}, {y}, {z} is not a valid position.");
				}

				return Voxels[GetVoxelIndex(x, y, z)];
			}
			set
			{
				if (!IsValidPosition(x, y, z))
				{
					throw new IndexOutOfRangeException($"{x}, {y}, {z} is not a valid position.");
				}

				Voxels[GetVoxelIndex(x, y, z)] = value;
			}
		}

		public VoxelData this[int index]
		{
			get => Voxels[index];
			set => Voxels[index] = value;
		}

		public Face GetFace(int x, int y, int z)
		{
			var faceIndex = GetVoxelIndex(x, y, z);

			if (!IsValidPosition(x, y, z))
			{
				throw new IndexOutOfRangeException($"{x}, {y}, {z} is not a valid position.");
			}

			return Faces[faceIndex];
		}

		public void SetFace(int x, int y, int z, Face face)
		{
			var faceIndex = GetVoxelIndex(x, y, z);

			if (!IsValidPosition(x, y, z))
			{
				throw new IndexOutOfRangeException($"{x}, {y}, {z} is not a valid position.");
			}

			Faces[faceIndex] = face;
		}

		public readonly bool IsValidPosition(int x, int y, int z)
		{
			return 0 <= x && x < Width && 0 <= y && y < Height && 0 <= z && z < Depth;
		}

		public int GetVoxelIndex(int x, int y, int z)
		{
			int chunkStartX = x / Chunk.ChunkSize;
			int chunkStartY = y / Chunk.ChunkSize;
			int chunkStartZ = z / Chunk.ChunkSize;
			int startChunkIndex = (chunkStartZ + chunkStartY * Depth / Chunk.ChunkSize +
			                       chunkStartX * (Depth * Height / Chunk.ChunkSizeSquared)) * Chunk.ChunkSizeCubed;
			int localIndex = x % Chunk.ChunkSize * Chunk.ChunkSizeSquared + y % Chunk.ChunkSize * Chunk.ChunkSize + z % Chunk.ChunkSize;
			return startChunkIndex + localIndex;
		}

		public int GetChunkIndex(int x, int y, int z)
		{
			int chunkX = x / Chunk.ChunkSize;
			int chunkY = y / Chunk.ChunkSize;
			int chunkZ = z / Chunk.ChunkSize;
			return chunkX * Height * Depth / Chunk.ChunkSizeSquared + chunkY * Depth / Chunk.ChunkSize + chunkZ;
		}

		public async UniTask<NativeArray<byte>> SerializeAsync()
		{
			const int chunksPerFrame = 8;
			var buffer = new NativeList<byte>(Allocator.Persistent);
			buffer.AddInt(Width);
			buffer.AddInt(Height);
			buffer.AddInt(Depth);
			var snapshotChunks = new NativeArray<NativeArray<VoxelData>>(chunksPerFrame, Allocator.Persistent);

			for (int i = 0; i < snapshotChunks.Length; i++)
			{
				snapshotChunks[i] = new NativeArray<VoxelData>(Chunk.ChunkSizeCubed, Allocator.Persistent);
			}

			for (int i = 0; i < ChunkCount; i += chunksPerFrame)
			{
				int end = Math.Min(i + chunksPerFrame, ChunkCount);
				for (int j = i; j < end; j++)
				{
					CopyChunk(snapshotChunks[j % chunksPerFrame], j);
				}
				
				for (int j = i; j < end; j++)
				{
					await WriteSerializedChunk(buffer, snapshotChunks[j % chunksPerFrame]);
				}
			}

			for (var i = 0; i < snapshotChunks.Length; i++)
			{
				snapshotChunks[i].Dispose();
			}

			snapshotChunks.Dispose();
			
			return buffer.AsArray();
		}

		private UniTask WriteSerializedChunk(NativeList<byte> buffer, NativeArray<VoxelData> voxels)
		{
			var jobHandle = new SerializeChunkJob(buffer, voxels, VoxelData.DefaultInner.Color);
			return jobHandle.Schedule().ToUniTask(PlayerLoopTiming.Update);
		}

		private void CopyChunk(NativeArray<VoxelData> chunkVoxels, int chunkIndex)
		{
			for (var i = 0; i < Chunk.ChunkSizeCubed; i++)
			{
				var voxelIndex = chunkIndex * Chunk.ChunkSizeCubed + i;
				chunkVoxels[i] = Voxels[voxelIndex];
			}
		}

		public void Dispose()
		{
			Voxels.Dispose();
			Faces.Dispose();
		}
	}
}
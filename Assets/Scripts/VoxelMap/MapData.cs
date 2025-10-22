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

		public byte[] Serialize()
		{
			var chunkBuffers = new NativeList<byte>[ChunkCount];
			var jobHandles = new NativeArray<JobHandle>(ChunkCount, Allocator.TempJob);

			for (int i = 0; i < ChunkCount; i++)
			{
				chunkBuffers[i] = new NativeList<byte>(Allocator.TempJob);
				var job = new SerializeChunkJob(chunkBuffers[i], Voxels.AsReadOnly(), i, VoxelData.DefaultInner.Color);
				jobHandles[i] = job.Schedule();
			}

			JobHandle.CompleteAll(jobHandles);

			jobHandles.Dispose();

			using var finalBuffer = new NativeList<byte>(Allocator.Temp);
			finalBuffer.AddInt(Width);
			finalBuffer.AddInt(Height);
			finalBuffer.AddInt(Depth);

			foreach (var chunkBuffer in chunkBuffers)
			{
				finalBuffer.AddRange(chunkBuffer.AsArray());
				chunkBuffer.Dispose();
			}

			byte[] result = finalBuffer.AsArray().ToArray();
			return result;
		}

		public async UniTask<byte[]> SerializeAsync()
		{
			var chunkBuffers = new NativeList<byte>[ChunkCount];
			var tasks = new UniTask[ChunkCount];

			for (int i = 0; i < ChunkCount; i++)
			{
				chunkBuffers[i] = new NativeList<byte>(Allocator.Persistent);
				var job = new SerializeChunkJob(chunkBuffers[i], Voxels.AsReadOnly(), i, VoxelData.DefaultInner.Color);
				tasks[i] = job.Schedule().ToUniTask(PlayerLoopTiming.Update);
			}

			await UniTask.WhenAll(tasks);

			using var finalBuffer = new NativeList<byte>(Allocator.Temp);
			finalBuffer.AddInt(Width);
			finalBuffer.AddInt(Height);
			finalBuffer.AddInt(Depth);

			foreach (var chunkBuffer in chunkBuffers)
			{
				finalBuffer.AddRange(chunkBuffer.AsArray());
				chunkBuffer.Dispose();
			}

			byte[] result = finalBuffer.AsArray().ToArray();
			return result;
		}

		public void Dispose()
		{
			Voxels.Dispose();
			Faces.Dispose();
		}

		public struct Readonly
		{
			public int ChunkCount => Width * Depth * Height / Chunk.ChunkSizeCubed;
			public readonly int Width;
			public readonly int Depth;
			public readonly int Height;
			[NativeDisableContainerSafetyRestriction]
			internal NativeArray<Face>.ReadOnly Faces;
			[NativeDisableContainerSafetyRestriction]
			internal NativeArray<VoxelData>.ReadOnly Voxels;
			
			public Readonly(MapData mapData)
			{
				Width = mapData.Width;
				Depth = mapData.Depth;
				Height = mapData.Height;
				Faces = mapData.Faces.AsReadOnly();
				Voxels = mapData.Voxels.AsReadOnly();
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
			}

			public VoxelData this[int index] => Voxels[index];

			public Face GetFace(int x, int y, int z)
			{
				var faceIndex = GetVoxelIndex(x, y, z);

				if (!IsValidPosition(x, y, z))
				{
					throw new IndexOutOfRangeException($"{x}, {y}, {z} is not a valid position.");
				}

				return Faces[faceIndex];
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

			public byte[] Serialize()
			{
				var chunkBuffers = new NativeList<byte>[ChunkCount];
				var jobHandles = new NativeArray<JobHandle>(ChunkCount, Allocator.TempJob);

				for (int i = 0; i < ChunkCount; i++)
				{
					chunkBuffers[i] = new NativeList<byte>(Allocator.TempJob);
					var job = new SerializeChunkJob(chunkBuffers[i], Voxels, i, VoxelData.DefaultInner.Color);
					jobHandles[i] = job.Schedule();
				}

				JobHandle.CompleteAll(jobHandles);

				jobHandles.Dispose();

				using var finalBuffer = new NativeList<byte>(Allocator.Temp);
				finalBuffer.AddInt(Width);
				finalBuffer.AddInt(Height);
				finalBuffer.AddInt(Depth);

				foreach (var chunkBuffer in chunkBuffers)
				{
					finalBuffer.AddRange(chunkBuffer.AsArray());
					chunkBuffer.Dispose();
				}

				byte[] result = finalBuffer.AsArray().ToArray();
				return result;
			}

			public async UniTask<byte[]> SerializeAsync()
			{
				var chunkBuffers = new NativeList<byte>[ChunkCount];
				var tasks = new UniTask[ChunkCount];

				for (int i = 0; i < ChunkCount; i++)
				{
					chunkBuffers[i] = new NativeList<byte>(Allocator.Persistent);
					var job = new SerializeChunkJob(chunkBuffers[i], Voxels, i, VoxelData.DefaultInner.Color);
					tasks[i] = job.Schedule().ToUniTask(PlayerLoopTiming.Update);
				}

				await UniTask.WhenAll(tasks);

				using var finalBuffer = new NativeList<byte>(Allocator.Temp);
				finalBuffer.AddInt(Width);
				finalBuffer.AddInt(Height);
				finalBuffer.AddInt(Depth);

				foreach (var chunkBuffer in chunkBuffers)
				{
					finalBuffer.AddRange(chunkBuffer.AsArray());
					chunkBuffer.Dispose();
				}

				byte[] result = finalBuffer.AsArray().ToArray();
				return result;
			}
		}
	}
}
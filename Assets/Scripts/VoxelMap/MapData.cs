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
		public readonly ushort Width;
		public readonly ushort Depth;
		public readonly ushort Height;
		[NativeDisableContainerSafetyRestriction]
		internal NativeArray<Face> Faces;
		[NativeDisableContainerSafetyRestriction]
		internal NativeArray<VoxelData> Voxels;

		public MapData(NativeArray<VoxelData> voxels, ushort width, ushort height, ushort depth)
		{
			Width = width;
			Height = height;
			Depth = depth;
			Faces = new NativeArray<Face>(voxels.Length, Allocator.Persistent);
			Voxels = voxels;
		}

		public VoxelData this[ushort x, ushort y, ushort z]
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

		public Face GetFace(ushort x, ushort y, ushort z)
		{
			var faceIndex = GetVoxelIndex(x, y, z);

			if (!IsValidPosition(x, y, z))
			{
				throw new IndexOutOfRangeException($"{x}, {y}, {z} is not a valid position.");
			}

			return Faces[faceIndex];
		}

		public void SetFace(ushort x, ushort y, ushort z, Face face)
		{
			var faceIndex = GetVoxelIndex(x, y, z);

			if (!IsValidPosition(x, y, z))
			{
				throw new IndexOutOfRangeException($"{x}, {y}, {z} is not a valid position.");
			}

			Faces[faceIndex] = face;
		}

		public readonly bool IsValidPosition(ushort x, ushort y, ushort z)
		{
			return x < Width && y < Height && z < Depth;
		}

		public int GetVoxelIndex(ushort x, ushort y, ushort z)
		{
			int chunkStartX = x / Chunk.ChunkSize;
			int chunkStartY = y / Chunk.ChunkSize;
			int chunkStartZ = z / Chunk.ChunkSize;
			int startChunkIndex = (chunkStartZ + chunkStartY * Depth / Chunk.ChunkSize +
			                       chunkStartX * (Depth * Height / Chunk.ChunkSizeSquared)) * Chunk.ChunkSizeCubed;
			int localIndex = x % Chunk.ChunkSize * Chunk.ChunkSizeSquared + y % Chunk.ChunkSize * Chunk.ChunkSize + z % Chunk.ChunkSize;
			return startChunkIndex + localIndex;
		}

		public int GetChunkIndex(ushort x, ushort y, ushort z)
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
			private int ChunkCount => _width * _depth * _height / Chunk.ChunkSizeCubed;
			private readonly int _width;
			private readonly int _depth;
			private readonly int _height;
			[NativeDisableContainerSafetyRestriction]
			private readonly NativeArray<VoxelData>.ReadOnly _voxels;
			[NativeDisableContainerSafetyRestriction]
			private NativeArray<Face>.ReadOnly _faces;

			public Readonly(MapData mapData)
			{
				_width = mapData.Width;
				_depth = mapData.Depth;
				_height = mapData.Height;
				_voxels = mapData.Voxels.AsReadOnly();
				_faces = mapData.Faces.AsReadOnly();
			}

			public Face GetFace(ushort x, ushort y, ushort z)
			{
				var faceIndex = GetVoxelIndex(x, y, z);

				if (!IsValidPosition(x, y, z))
				{
					throw new IndexOutOfRangeException($"{x}, {y}, {z} is not a valid position.");
				}

				return _faces[faceIndex];
			}

			public async UniTask<byte[]> SerializeAsync()
			{
				var chunkBuffers = new NativeList<byte>[ChunkCount];
				var tasks = new UniTask[ChunkCount];

				for (int i = 0; i < ChunkCount; i++)
				{
					chunkBuffers[i] = new NativeList<byte>(Allocator.Persistent);
					var job = new SerializeChunkJob(chunkBuffers[i], _voxels, i, VoxelData.DefaultInner.Color);
					tasks[i] = job.Schedule().ToUniTask(PlayerLoopTiming.Update);
				}

				await UniTask.WhenAll(tasks);

				using var finalBuffer = new NativeList<byte>(Allocator.Temp);
				finalBuffer.AddInt(_width);
				finalBuffer.AddInt(_height);
				finalBuffer.AddInt(_depth);

				foreach (var chunkBuffer in chunkBuffers)
				{
					finalBuffer.AddRange(chunkBuffer.AsArray());
					chunkBuffer.Dispose();
				}

				byte[] result = finalBuffer.AsArray().ToArray();
				return result;
			}

			private int GetVoxelIndex(ushort x, ushort y, ushort z)
			{
				int chunkStartX = x / Chunk.ChunkSize;
				int chunkStartY = y / Chunk.ChunkSize;
				int chunkStartZ = z / Chunk.ChunkSize;
				int startChunkIndex = (chunkStartZ + chunkStartY * _depth / Chunk.ChunkSize +
				                       chunkStartX * (_depth * _height / Chunk.ChunkSizeSquared)) * Chunk.ChunkSizeCubed;
				int localIndex = x % Chunk.ChunkSize * Chunk.ChunkSizeSquared + y % Chunk.ChunkSize * Chunk.ChunkSize + z % Chunk.ChunkSize;
				return startChunkIndex + localIndex;
			}

			private readonly bool IsValidPosition(ushort x, ushort y, ushort z)
			{
				return x < _width && y < _height && z < _depth;
			}
		}
	}
}
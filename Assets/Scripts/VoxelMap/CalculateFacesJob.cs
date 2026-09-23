using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
namespace VoxelMap
{
	[BurstCompile]
	[StructLayout(LayoutKind.Sequential)]
	public struct CalculateFacesJob : IJobFor
	{
		[NativeDisableContainerSafetyRestriction]
		private NativeArray<Face> _faces;
		[NativeDisableContainerSafetyRestriction]
		private readonly NativeArray<VoxelData> _voxels;

		private readonly int _width;
		private readonly int _height;
		private readonly int _death;

		private NativeArray<int> _facesCountPerChunk;

		public CalculateFacesJob(MapData mapData, NativeArray<int> facesCountPerChunk)
		{
			_voxels = mapData.Voxels;
			_faces = mapData.Faces;
			_width = mapData.Width;
			_height = mapData.Height;
			_death = mapData.Depth;
			_facesCountPerChunk = facesCountPerChunk;
		}

		public void Execute(int chunkIndex)
		{
			for (var i = 0; i < Chunk.ChunkSizeCubed; i++)
			{
				int voxelIndex = chunkIndex * Chunk.ChunkSizeCubed + i;
				_faces[voxelIndex] = Face.None;

				if (!_voxels[voxelIndex].IsSolid())
				{
					continue;
				}

				int chunkOffsetX = chunkIndex % (_death / Chunk.ChunkSize) * Chunk.ChunkSize;
				int chunkOffsetY = chunkIndex / (_death / Chunk.ChunkSize) % (_height / Chunk.ChunkSize) * Chunk.ChunkSize;
				int chunkOffsetZ = chunkIndex / (_death * _height / Chunk.ChunkSizeSquared) * Chunk.ChunkSize;
				int z = i % Chunk.ChunkSize + chunkOffsetX;
				int y = i / Chunk.ChunkSize % Chunk.ChunkSize + chunkOffsetY;
				int x = i / Chunk.ChunkSizeSquared + chunkOffsetZ;

				if (CheckTopFace(x, y, z))
				{
					_faces[voxelIndex] |= Face.Top;
					_facesCountPerChunk[chunkIndex]++;
				}

				if (CheckBottomFace(x, y, z))
				{
					_faces[voxelIndex] |= Face.Bottom;
					_facesCountPerChunk[chunkIndex]++;
				}

				if (CheckFrontFace(x, y, z))
				{
					_faces[voxelIndex] |= Face.Front;
					_facesCountPerChunk[chunkIndex]++;
				}

				if (CheckBackFace(x, y, z))
				{
					_faces[voxelIndex] |= Face.Back;
					_facesCountPerChunk[chunkIndex]++;
				}

				if (CheckRightFace(x, y, z))
				{
					_faces[voxelIndex] |= Face.Right;
					_facesCountPerChunk[chunkIndex]++;
				}

				if (CheckLeftFace(x, y, z))
				{
					_faces[voxelIndex] |= Face.Left; 
					_facesCountPerChunk[chunkIndex]++;
				}
			}
		}


		private bool CheckTopFace(int x, int y, int z)
		{
			return !IsValidPosition(x, y + 1, z) || !_voxels[GetIndex(x, y + 1, z)].IsSolid();
		}

		private bool CheckBottomFace(int x, int y, int z)
		{
			if (!IsValidPosition(x, y - 1, z))
			{
				return false;
			}

			return !_voxels[GetIndex(x, y - 1, z)].IsSolid();
		}

		private bool CheckFrontFace(int x, int y, int z)
		{
			if (!IsValidPosition(x, y, z + 1))
			{
				return false;
			}

			return !_voxels[GetIndex(x, y, z + 1)].IsSolid();
		}

		private bool CheckBackFace(int x, int y, int z)
		{
			if (!IsValidPosition(x, y, z - 1))
			{
				return false;
			}

			return !_voxels[GetIndex(x, y, z - 1)].IsSolid();
		}

		private bool CheckRightFace(int x, int y, int z)
		{
			if (!IsValidPosition(x + 1, y, z))
			{
				return false;
			}

			return !_voxels[GetIndex(x + 1, y, z)].IsSolid();
		}

		private bool CheckLeftFace(int x, int y, int z)
		{
			if (!IsValidPosition(x - 1, y, z))
			{
				return false;
			}

			return !_voxels[GetIndex(x - 1, y, z)].IsSolid();
		}

		private bool IsValidPosition(int x, int y, int z)
		{
			return 0 <= x && x < _width && 0 <= y && y < _height && 0 <= z && z < _death;
		}

		private int GetIndex(int x, int y, int z)
		{
			int chunkStartX = x / Chunk.ChunkSize;
			int chunkStartY = y / Chunk.ChunkSize;
			int chunkStartZ = z / Chunk.ChunkSize;
			int startChunkIndex = (chunkStartZ + chunkStartY * _death / Chunk.ChunkSize +
			                       chunkStartX * (_death * _height / Chunk.ChunkSizeSquared)) * Chunk.ChunkSizeCubed;
			int localIndex = x % Chunk.ChunkSize * Chunk.ChunkSizeSquared + y % Chunk.ChunkSize * Chunk.ChunkSize + z % Chunk.ChunkSize;
			return startChunkIndex + localIndex;
		}
	}
}
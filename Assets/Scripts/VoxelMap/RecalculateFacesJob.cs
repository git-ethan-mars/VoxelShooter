using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
namespace VoxelMap
{
	[BurstCompile]
	public struct RecalculateFacesJob : IJob
	{
		private NativeArray<Face> _neighboursToRegenerate;
		private NativeHashMap<int, int> _faceCountChangesByChunk;

		[ReadOnly]
		private NativeList<Voxel> _voxels;

		private MapData _mapData;
		private readonly int _chunkIndex;

		public RecalculateFacesJob(NativeList<Voxel> voxels, MapData mapData, int chunkIndex, NativeArray<Face> neighboursToRegenerate,
			NativeHashMap<int, int> faceCountChangesByChunk)
		{
			_voxels = voxels;
			_mapData = mapData;
			_chunkIndex = chunkIndex;
			_neighboursToRegenerate = neighboursToRegenerate;
			_faceCountChangesByChunk = faceCountChangesByChunk;
		}

		public void Execute()
		{
			_faceCountChangesByChunk[_chunkIndex] = 0;

			for (var i = 0; i < _voxels.Length; i++)
			{
				int x = _voxels[i].Position.x;
				int y = _voxels[i].Position.y;
				int z = _voxels[i].Position.z;

				_mapData[x, y, z] = _voxels[i].Data;

				Face oldFaces = _mapData.GetFace(x, y, z);
				Face newFaces = CalculateFaces(x, y, z);

				if (oldFaces != newFaces)
				{
					_mapData.SetFace(x, y, z, newFaces);
					_faceCountChangesByChunk[_chunkIndex] += GetFaceCount(newFaces) - GetFaceCount(oldFaces);
				}

				UpdateNeighbourFaces(x, y, z);
				SetNeighbourChunksForRegeneration(x, y, z);
			}
		}

		private Face CalculateFaces(int x, int y, int z)
		{
			var face = Face.None;

			if (_mapData[x, y, z].IsSolid())
			{
				if (HasTopFace(x, y, z))
				{
					face |= Face.Top;
				}

				if (HasBottomFace(x, y, z))
				{
					face |= Face.Bottom;
				}

				if (HasFrontFace(x, y, z))
				{
					face |= Face.Front;
				}

				if (HasBackFace(x, y, z))
				{
					face |= Face.Back;
				}

				if (HasRightFace(x, y, z))
				{
					face |= Face.Right;
				}

				if (HasLeftFace(x, y, z))
				{
					face |= Face.Left;
				}
			}

			return face;
		}

		private int GetFaceCount(Face faces)
		{
			int faceCount = 0;

			if (FaceExtensions.HasFlag(faces, Face.Top))
			{
				faceCount++;
			}
			if (FaceExtensions.HasFlag(faces, Face.Bottom))
			{
				faceCount++;
			}
			if (FaceExtensions.HasFlag(faces, Face.Front))
			{
				faceCount++;
			}
			if (FaceExtensions.HasFlag(faces, Face.Back))
			{
				faceCount++;
			}
			if (FaceExtensions.HasFlag(faces, Face.Right))
			{
				faceCount++;
			}
			if (FaceExtensions.HasFlag(faces, Face.Left))
			{
				faceCount++;
			}

			return faceCount;
		}

		private void SetNeighbourChunksForRegeneration(int x, int y, int z)
		{
			if (x % Chunk.ChunkSize == 0)
			{
				_neighboursToRegenerate[0] |= Face.Left;
			}
			if (x % Chunk.ChunkSize == Chunk.ChunkSize - 1)
			{
				_neighboursToRegenerate[0] |= Face.Right;
			}
			if (y % Chunk.ChunkSize == 0)
			{
				_neighboursToRegenerate[0] |= Face.Bottom;
			}
			if (y % Chunk.ChunkSize == Chunk.ChunkSize - 1)
			{
				_neighboursToRegenerate[0] |= Face.Top;
			}
			if (z % Chunk.ChunkSize == 0)
			{
				_neighboursToRegenerate[0] |= Face.Back;
			}
			if (z % Chunk.ChunkSize == Chunk.ChunkSize - 1)
			{
				_neighboursToRegenerate[0] |= Face.Front;
			}
		}

		private void UpdateNeighbourFaces(int x, int y, int z)
		{
			UpdateSingleNeighbour(x, y + 1, z, Face.Bottom, HasBottomFace(x, y + 1, z));
			UpdateSingleNeighbour(x, y - 1, z, Face.Top, HasTopFace(x, y - 1, z));
			UpdateSingleNeighbour(x, y, z + 1, Face.Back, HasBackFace(x, y, z + 1));
			UpdateSingleNeighbour(x, y, z - 1, Face.Front, HasFrontFace(x, y, z - 1));
			UpdateSingleNeighbour(x + 1, y, z, Face.Left, HasLeftFace(x + 1, y, z));
			UpdateSingleNeighbour(x - 1, y, z, Face.Right, HasRightFace(x - 1, y, z));
		}

		private void UpdateSingleNeighbour(int nx, int ny, int nz, Face faceFlag, bool shouldHaveFace)
		{
			if (!_mapData.IsValidPosition(nx, ny, nz) || !_mapData[nx, ny, nz].IsSolid())
				return;

			int chunkIndex = _mapData.GetChunkIndex(nx, ny, nz);
			Face oldFace = _mapData.GetFace(nx, ny, nz);
			bool currentlyHasFace = FaceExtensions.HasFlag(oldFace, faceFlag);

			if (currentlyHasFace != shouldHaveFace)
			{
				Face newFace = oldFace;
				if (shouldHaveFace)
				{
					newFace |= faceFlag;
				}
				else
				{
					newFace &= ~faceFlag;
				}

				_mapData.SetFace(nx, ny, nz, newFace);

				if (!_faceCountChangesByChunk.ContainsKey(chunkIndex))
				{
					_faceCountChangesByChunk[chunkIndex] = 0;
				}

				_faceCountChangesByChunk[chunkIndex] += shouldHaveFace ? 1 : -1;
			}
		}

		private bool HasTopFace(int x, int y, int z)
		{
			return !_mapData.IsValidPosition(x, y + 1, z) || !_mapData[x, y + 1, z].IsSolid();
		}

		private bool HasBottomFace(int x, int y, int z)
		{
			if (!_mapData.IsValidPosition(x, y - 1, z))
			{
				return false;
			}

			return !_mapData[x, y - 1, z].IsSolid();
		}

		private bool HasFrontFace(int x, int y, int z)
		{
			if (!_mapData.IsValidPosition(x, y, z + 1))
			{
				return false;
			}

			return !_mapData[x, y, z + 1].IsSolid();
		}

		private bool HasBackFace(int x, int y, int z)
		{
			if (!_mapData.IsValidPosition(x, y, z - 1))
			{
				return false;
			}

			return !_mapData[x, y, z - 1].IsSolid();
		}

		private bool HasRightFace(int x, int y, int z)
		{
			if (!_mapData.IsValidPosition(x + 1, y, z))
			{
				return false;
			}

			return !_mapData[x + 1, y, z].IsSolid();
		}

		private bool HasLeftFace(int x, int y, int z)
		{
			if (!_mapData.IsValidPosition(x - 1, y, z))
			{
				return false;
			}

			return !_mapData[x - 1, y, z].IsSolid();
		}
	}
}
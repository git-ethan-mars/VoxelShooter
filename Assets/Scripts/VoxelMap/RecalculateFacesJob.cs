using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
namespace VoxelMap
{
	[BurstCompile]
	public struct RecalculateFacesJob : IJob
	{
		private NativeReference<Face> _regeneratingRegeneratingNeighbours;
		private NativeHashMap<int, int> _faceCountChangesByChunk;

		[ReadOnly]
		private NativeList<Voxel> _voxels;

		private MapData _mapData;
		private readonly int _chunkIndex;

		public RecalculateFacesJob(NativeList<Voxel> voxels, MapData mapData, int chunkIndex, NativeReference<Face> regeneratingNeighbours,
			NativeHashMap<int, int> faceCountChangesByChunk)
		{
			_voxels = voxels;
			_mapData = mapData;
			_chunkIndex = chunkIndex;
			_regeneratingRegeneratingNeighbours = regeneratingNeighbours;
			_faceCountChangesByChunk = faceCountChangesByChunk;
		}

		public void Execute()
		{
			_faceCountChangesByChunk[_chunkIndex] = 0;

			for (var i = 0; i < _voxels.Length; i++)
			{
				ushort x = _voxels[i].Position.x;
				ushort y = _voxels[i].Position.y;
				ushort z = _voxels[i].Position.z;

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

		private Face CalculateFaces(ushort x, ushort y, ushort z)
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
				_regeneratingRegeneratingNeighbours.Value |= Face.Left;
			}
			if (x % Chunk.ChunkSize == Chunk.ChunkSize - 1)
			{
				_regeneratingRegeneratingNeighbours.Value |= Face.Right;
			}
			if (y % Chunk.ChunkSize == 0)
			{
				_regeneratingRegeneratingNeighbours.Value |= Face.Bottom;
			}
			if (y % Chunk.ChunkSize == Chunk.ChunkSize - 1)
			{
				_regeneratingRegeneratingNeighbours.Value |= Face.Top;
			}
			if (z % Chunk.ChunkSize == 0)
			{
				_regeneratingRegeneratingNeighbours.Value |= Face.Back;
			}
			if (z % Chunk.ChunkSize == Chunk.ChunkSize - 1)
			{
				_regeneratingRegeneratingNeighbours.Value |= Face.Front;
			}
		}

		private void UpdateNeighbourFaces(ushort x, ushort y, ushort z)
		{
			UpdateSingleNeighbour(x, (ushort)(y + 1), z, Face.Bottom, HasBottomFace(x, (ushort)(y + 1), z));
			UpdateSingleNeighbour(x, (ushort)(y - 1), z, Face.Top, HasTopFace(x, (ushort)(y - 1), z));
			UpdateSingleNeighbour(x, y, (ushort)(z + 1), Face.Back, HasBackFace(x, y, (ushort)(z + 1)));
			UpdateSingleNeighbour(x, y, (ushort)(z - 1), Face.Front, HasFrontFace(x, y, (ushort)(z - 1)));
			UpdateSingleNeighbour((ushort)(x + 1), y, z, Face.Left, HasLeftFace((ushort)(x + 1), y, z));
			UpdateSingleNeighbour((ushort)(x - 1), y, z, Face.Right, HasRightFace((ushort)(x - 1), y, z));
		}

		private void UpdateSingleNeighbour(ushort nx, ushort ny, ushort nz, Face faceFlag, bool shouldHaveFace)
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

		private bool HasTopFace(ushort x, ushort y, ushort z)
		{
			return !_mapData.IsValidPosition(x, (ushort)(y + 1), z) || !_mapData[x, (ushort)(y + 1), z].IsSolid();
		}

		private bool HasBottomFace(ushort x, ushort y, ushort z)
		{
			if (!_mapData.IsValidPosition(x, (ushort)(y - 1), z))
			{
				return false;
			}

			return !_mapData[x, (ushort)(y - 1), z].IsSolid();
		}

		private bool HasFrontFace(ushort x, ushort y, ushort z)
		{
			if (!_mapData.IsValidPosition(x, y, (ushort)(z + 1)))
			{
				return false;
			}

			return !_mapData[x, y, (ushort)(z + 1)].IsSolid();
		}

		private bool HasBackFace(ushort x, ushort y, ushort z)
		{
			if (!_mapData.IsValidPosition(x, y, (ushort)(z - 1)))
			{
				return false;
			}

			return !_mapData[x, y, (ushort)(z - 1)].IsSolid();
		}

		private bool HasRightFace(ushort x, ushort y, ushort z)
		{
			if (!_mapData.IsValidPosition((ushort)(x + 1), y, z))
			{
				return false;
			}

			return !_mapData[(ushort)(x + 1), y, z].IsSolid();
		}

		private bool HasLeftFace(ushort x, ushort y, ushort z)
		{
			if (!_mapData.IsValidPosition((ushort)(x - 1), y, z))
			{
				return false;
			}

			return !_mapData[(ushort)(x - 1), y, z].IsSolid();
		}
	}
}
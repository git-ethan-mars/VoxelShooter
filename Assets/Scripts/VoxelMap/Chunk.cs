using System;
using System.Collections.Generic;
using Common;
using UnityEngine;

namespace VoxelMap
{
	public class Chunk
	{
		private readonly Dictionary<ChunkNeighbourType, Chunk> _neighbours = new();
		private readonly Faces[] _faces;
		private readonly Mesh _mesh;
		private readonly MeshData _meshData;
		private readonly ChunkData _chunkData;
		private readonly MeshFilter _meshFilter;
		private readonly MeshCollider _meshCollider;
		
		public Chunk(GameObject chunkMeshObject, ChunkData chunkData, MeshData meshData, Faces[] faces)
		{
			_chunkData = chunkData;
			_meshData = meshData;
			_faces = faces;
			_mesh = new Mesh();
			_meshFilter = chunkMeshObject.GetComponent<MeshFilter>();
			_meshCollider = chunkMeshObject.GetComponent<MeshCollider>();
			ApplyMesh();
		}

		public void SetNeighbour(ChunkNeighbourType neighbourType, Chunk chunk)
		{
			_neighbours[neighbourType] = chunk;
		}

		public void SpawnBlocks(List<BlockDataWithPosition> blocks)
		{
			var neighbourChunksToRegenerate = Faces.None;
			for (var i = 0; i < blocks.Count; i++)
			{
				var x = blocks[i].Position.x;
				var y = blocks[i].Position.y;
				var z = blocks[i].Position.z;
				_chunkData.SetBlock(x, y, z, blocks[i].BlockData, PositionType.Local);
				UpdateBlockFaces(x, y, z);
				SetNeighboursFaces(blocks[i].Position);
				if (z == ChunkData.ChunkSize - 1 && _neighbours.TryGetValue(ChunkNeighbourType.Front, out var frontNeighbour))
				{
					frontNeighbour.UpdateBlockFaces(x, y, 0);
					neighbourChunksToRegenerate |= Faces.Front;
				}

				if (z == 0 && _neighbours.TryGetValue(ChunkNeighbourType.Back, out var backNeighbour))
				{
					backNeighbour.UpdateBlockFaces(x, y, ChunkData.ChunkSize - 1);
					neighbourChunksToRegenerate |= Faces.Back;
				}

				if (y == ChunkData.ChunkSize - 1)
				{
					if (_neighbours.TryGetValue(ChunkNeighbourType.Up, out var upperNeighbour))
					{
						upperNeighbour.UpdateBlockFaces(x, 0, z);
					}

					neighbourChunksToRegenerate |= Faces.Top;
				}

				if (y == 0 && _neighbours.TryGetValue(ChunkNeighbourType.Down, out var lowerNeighbour))
				{
					lowerNeighbour.UpdateBlockFaces(x, ChunkData.ChunkSize - 1, z);
					neighbourChunksToRegenerate |= Faces.Bottom;
				}

				if (x == ChunkData.ChunkSize - 1 && _neighbours.TryGetValue(ChunkNeighbourType.Right, out var rightNeighbour))
				{
					rightNeighbour.UpdateBlockFaces(0, y, z);
					neighbourChunksToRegenerate |= Faces.Right;
				}
				else if (x == 0 && _neighbours.TryGetValue(ChunkNeighbourType.Left, out var leftNeighbour))
				{
					leftNeighbour.UpdateBlockFaces(ChunkData.ChunkSize - 1, y, z);
					neighbourChunksToRegenerate |= Faces.Left;
				}
			}

			RegenerateMesh();

			foreach (ChunkNeighbourType chunkNeighbour in Enum.GetValues(typeof(ChunkNeighbourType)))
			{
				if (neighbourChunksToRegenerate.HasFlag(chunkNeighbour))
				{
					_neighbours[chunkNeighbour].RegenerateMesh();
				}
			}
		}

		private void ApplyMesh()
		{
			_mesh.SetVertices(_meshData.Vertices);
			_mesh.SetTriangles(_meshData.Triangles, 0);
			_mesh.SetColors(_meshData.Colors);
			_mesh.SetNormals(_meshData.Normals);
			if (_meshData.Vertices.Count == 0)
			{
				_meshCollider.sharedMesh = null;
			}
			else
			{
				_meshFilter.mesh = _mesh;
				_meshCollider.sharedMesh = _mesh;
			}
		}

		private void SetNeighboursFaces(Vector3Int blockPosition)
		{
			if (ChunkData.IsValidPosition(blockPosition.x + 1, blockPosition.y, blockPosition.z))
			{
				UpdateBlockFaces(blockPosition.x + 1, blockPosition.y, blockPosition.z);
			}

			if (ChunkData.IsValidPosition(blockPosition.x - 1, blockPosition.y, blockPosition.z))
			{
				UpdateBlockFaces(blockPosition.x - 1, blockPosition.y, blockPosition.z);
			}

			if (ChunkData.IsValidPosition(blockPosition.x, blockPosition.y + 1, blockPosition.z))
			{
				UpdateBlockFaces(blockPosition.x, blockPosition.y + 1, blockPosition.z);
			}

			if (ChunkData.IsValidPosition(blockPosition.x, blockPosition.y - 1, blockPosition.z))
			{
				UpdateBlockFaces(blockPosition.x, blockPosition.y - 1, blockPosition.z);
			}

			if (ChunkData.IsValidPosition(blockPosition.x, blockPosition.y, blockPosition.z + 1))
			{
				UpdateBlockFaces(blockPosition.x, blockPosition.y, blockPosition.z + 1);
			}

			if (ChunkData.IsValidPosition(blockPosition.x, blockPosition.y, blockPosition.z - 1))
			{
				UpdateBlockFaces(blockPosition.x, blockPosition.y, blockPosition.z - 1);
			}
		}

		private void RegenerateMesh()
		{
			_mesh.Clear();
			_meshData.Vertices.Clear();
			_meshData.Triangles.Clear();
			_meshData.Colors.Clear();
			_mesh.Clear();
			_meshData.Normals.Clear();
			for (var x = 0; x < ChunkData.ChunkSize; x++)
			{
				for (var y = 0; y < ChunkData.ChunkSize; y++)
				{
					for (var z = 0; z < ChunkData.ChunkSize; z++)
					{
						if (GetFaces(x, y, z) == Faces.None)
						{
							continue;
						}

						var color = _chunkData.GetBlock(x, y, z, PositionType.Local).Color;
						if (GetFaces(x, y, z).HasFlag(Faces.Top))
						{
							ChunkGeneratorHelper.GenerateTopSide(x, y, z, color, _meshData.Vertices, _meshData.Normals,
								_meshData.Colors, _meshData.Triangles);
						}

						if (GetFaces(x, y, z).HasFlag(Faces.Bottom))
						{
							ChunkGeneratorHelper.GenerateBottomSide(x, y, z, color, _meshData.Vertices,
								_meshData.Normals,
								_meshData.Colors, _meshData.Triangles);
						}

						if (GetFaces(x, y, z).HasFlag(Faces.Front))
						{
							ChunkGeneratorHelper.GenerateFrontSide(x, y, z, color, _meshData.Vertices,
								_meshData.Normals,
								_meshData.Colors, _meshData.Triangles);
						}

						if (GetFaces(x, y, z).HasFlag(Faces.Back))
						{
							ChunkGeneratorHelper.GenerateBackSide(x, y, z, color, _meshData.Vertices, _meshData.Normals,
								_meshData.Colors, _meshData.Triangles);
						}

						if (GetFaces(x, y, z).HasFlag(Faces.Left))
						{
							ChunkGeneratorHelper.GenerateLeftSide(x, y, z, color, _meshData.Vertices, _meshData.Normals,
								_meshData.Colors, _meshData.Triangles);
						}

						if (GetFaces(x, y, z).HasFlag(Faces.Right))
						{
							ChunkGeneratorHelper.GenerateRightSide(x, y, z, color, _meshData.Vertices,
								_meshData.Normals,
								_meshData.Colors, _meshData.Triangles);
						}
					}
				}
			}

			ApplyMesh();
		}

		private void UpdateBlockFaces(int x, int y, int z)
		{
			var resultFaces = Faces.None;
			if (!_chunkData.GetBlock(x, y, z, PositionType.Local).IsSolid())
			{
				SetFaces(x, y, z, resultFaces);
				return;
			}

			var containsUpperChunk = _neighbours.TryGetValue(ChunkNeighbourType.Up, out var upperNeighbour);
			if (!ChunkData.IsValidPosition(x, y + 1, z) &&
			    (!containsUpperChunk || !upperNeighbour._chunkData.GetBlock(x, 0, z, PositionType.Local).IsSolid()) ||
			    ChunkData.IsValidPosition(x, y + 1, z) && !_chunkData.GetBlock(x, y + 1, z, PositionType.Local).IsSolid())
			{
				resultFaces |= Faces.Top;
			}

			var containsLowerChunk = _neighbours.TryGetValue(ChunkNeighbourType.Down, out var lowerNeighbour);
			if (!ChunkData.IsValidPosition(x, y - 1, z) && containsLowerChunk &&
			    !lowerNeighbour._chunkData.GetBlock(x, ChunkData.ChunkSize - 1, z, PositionType.Local).IsSolid() ||
			    ChunkData.IsValidPosition(x, y - 1, z) && !_chunkData.GetBlock(x, y - 1, z, PositionType.Local).IsSolid())
			{
				resultFaces |= Faces.Bottom;
			}

			var containsFrontChunk = _neighbours.TryGetValue(ChunkNeighbourType.Front, out var frontNeighbour);
			if (!ChunkData.IsValidPosition(x, y, z + 1) && containsFrontChunk &&
			    !frontNeighbour._chunkData.GetBlock(x, y, 0, PositionType.Local).IsSolid() ||
			    ChunkData.IsValidPosition(x, y, z + 1) && !_chunkData.GetBlock(x, y, z + 1, PositionType.Local).IsSolid())
			{
				resultFaces |= Faces.Front;
			}

			var containsBackChunk = _neighbours.TryGetValue(ChunkNeighbourType.Back, out var backNeighbour);
			if (!ChunkData.IsValidPosition(x, y, z - 1) && containsBackChunk &&
			    !backNeighbour._chunkData.GetBlock(x, y, ChunkData.ChunkSize - 1, PositionType.Local).IsSolid() ||
			    ChunkData.IsValidPosition(x, y, z - 1) && !_chunkData.GetBlock(x, y, z - 1, PositionType.Local).IsSolid())
			{
				resultFaces |= Faces.Back;
			}

			var containsRightChunk = _neighbours.TryGetValue(ChunkNeighbourType.Right, out var rightNeighbour);
			if (!ChunkData.IsValidPosition(x + 1, y, z) && containsRightChunk &&
			    !rightNeighbour._chunkData.GetBlock(0, y, z, PositionType.Local).IsSolid() ||
			    ChunkData.IsValidPosition(x + 1, y, z) && !_chunkData.GetBlock(x + 1, y, z, PositionType.Local).IsSolid())
			{
				resultFaces |= Faces.Right;
			}

			var containsLeftChunk = _neighbours.TryGetValue(ChunkNeighbourType.Left, out var leftNeighbour);
			if (!ChunkData.IsValidPosition(x - 1, y, z) && containsLeftChunk &&
			    !leftNeighbour._chunkData.GetBlock(ChunkData.ChunkSize - 1, y, z, PositionType.Local).IsSolid() ||
			    ChunkData.IsValidPosition(x - 1, y, z) && !_chunkData.GetBlock(x - 1, y, z, PositionType.Local).IsSolid())
			{
				resultFaces |= Faces.Left;
			}

			SetFaces(x, y, z, resultFaces);
		}

		private Faces GetFaces(int x, int y, int z)
		{
			return _faces[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z];
		}

		private void SetFaces(int x, int y, int z, Faces faces)
		{
			_faces[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z] = faces;
		}
	}
}
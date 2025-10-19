using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
namespace VoxelMap
{
	[BurstCompile]
	public struct RegenerateChunkJob : IJob
	{
		private MapData _mapData;
		private readonly int _chunkIndex;
		[NativeDisableContainerSafetyRestriction]
		private NativeArray<VertexData> _vertices;
		[NativeDisableContainerSafetyRestriction]
		private NativeArray<int> _indexes;
		private int _vertexCount;

		public RegenerateChunkJob(MapData mapData, int chunkIndex, NativeArray<VertexData> vertices, NativeArray<int> indexes)
		{
			_mapData = mapData;
			_chunkIndex = chunkIndex;
			_vertices = vertices;
			_indexes = indexes;
			_vertexCount = 0;
		}

		public void Execute()
		{
			for (var i = 0; i < Chunk.ChunkSizeCubed; i++)
			{
				int z = i % Chunk.ChunkSize;
				int y = i / Chunk.ChunkSize % Chunk.ChunkSize;
				int x = i / Chunk.ChunkSizeSquared;
				int voxelIndex = _chunkIndex * Chunk.ChunkSizeCubed + i;
				Face faces = _mapData.Faces[voxelIndex];
				Color32 color = _mapData[voxelIndex].Color;

				if (faces == Face.None)
				{
					continue;
				}

				GenerateVoxel(x, y, z, faces, color);
			}
		}

		private void GenerateVoxel(int x, int y, int z, Face faces, Color32 color)
		{
			if (FaceExtensions.HasFlag(faces, Face.Top))
			{
				GenerateTopSide(x, y, z, color);
			}

			if (FaceExtensions.HasFlag(faces, Face.Bottom))
			{
				GenerateBottomSide(x, y, z, color);
			}

			if (FaceExtensions.HasFlag(faces, Face.Front))
			{
				GenerateFrontSide(x, y, z, color);
			}

			if (FaceExtensions.HasFlag(faces, Face.Back))
			{
				GenerateBackSide(x, y, z, color);
			}

			if (FaceExtensions.HasFlag(faces, Face.Right))
			{
				GenerateRightSide(x, y, z, color);
			}

			if (FaceExtensions.HasFlag(faces, Face.Left))
			{
				GenerateLeftSide(x, y, z, color);
			}
		}

		private void GenerateTopSide(int x, int y, int z, Color32 color)
		{
			_vertices[_vertexCount] = new VertexData
			{
				Position = new Vector3(x, y + 1, z), Normal = Vector3.up, Red = color.r, Green = color.g, Blue =
					color.b,
				AmbientOcclusion = GetVertexAO(x, y, z, 0, 1, 0),
				UV = new Vector2(1, 1)
			};
			_vertices[_vertexCount + 1] = new VertexData
			{
				Position = new Vector3(x, y + 1, z + 1), Normal = Vector3.up, Red = color.r, Green = color.g, Blue =
					color.b,
				AmbientOcclusion = GetVertexAO(x, y, z, 0, 1, 1),
				UV = new Vector2(1, 0)
			};
			_vertices[_vertexCount + 2] = new VertexData
			{
				Position = new Vector3(x + 1, y + 1, z), Normal = Vector3.up, Red = color.r, Green = color.g, Blue =
					color.b,
				AmbientOcclusion = GetVertexAO(x, y, z, 1, 1, 0),
				UV = new Vector2(0, 1)
			};
			_vertices[_vertexCount + 3] = new VertexData
			{
				Position = new Vector3(x + 1, y + 1, z + 1), Normal = Vector3.up, Red = color.r, Green = color.g, Blue
					= color.b,
				AmbientOcclusion = GetVertexAO(x, y, z, 1, 1, 1),
				UV = new Vector2(0, 0)
			};

			_vertexCount += 4;

			if (_vertices[_vertexCount - 4].AmbientOcclusion + _vertices[_vertexCount - 1].AmbientOcclusion >
			    _vertices[_vertexCount - 3].AmbientOcclusion
			    + _vertices[_vertexCount - 2].AmbientOcclusion)
			{
				(_vertices[_vertexCount - 4], _vertices[_vertexCount - 3], _vertices[_vertexCount - 2], _vertices[_vertexCount - 1]) =
					(_vertices[_vertexCount - 2], _vertices[_vertexCount - 4], _vertices[_vertexCount - 1], _vertices[_vertexCount - 3]);
			}

			AddTriangles();
		}

		private void GenerateBottomSide(int x, int y, int z, Color32 color)
		{
			_vertices[_vertexCount] = new VertexData
			{
				Position = new Vector3(x, y, z), Normal = Vector3.down, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, 0, 0, 0),
				UV = new Vector2(1, 0)
			};
			_vertices[_vertexCount + 1] = new VertexData
			{
				Position = new Vector3(x + 1, y, z), Normal = Vector3.down, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, 1, 0, 0),
				UV = new Vector2(0, 0)
			};
			_vertices[_vertexCount + 2] = new VertexData
			{
				Position = new Vector3(x, y, z + 1), Normal = Vector3.down, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, 0, 0, 1),
				UV = new Vector2(1, 1)
			};
			_vertices[_vertexCount + 3] = new VertexData
			{
				Position = new Vector3(x + 1, y, z + 1), Normal = Vector3.down, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, 1, 0, 1),
				UV = new Vector2(0, 1)
			};

			_vertexCount += 4;

			if (_vertices[_vertexCount - 4].AmbientOcclusion + _vertices[_vertexCount - 1].AmbientOcclusion >
			    _vertices[_vertexCount - 3].AmbientOcclusion + _vertices[_vertexCount - 2].AmbientOcclusion)
			{
				(_vertices[_vertexCount - 4], _vertices[_vertexCount - 3], _vertices[_vertexCount - 2], _vertices[_vertexCount - 1]) =
					(_vertices[_vertexCount - 2], _vertices[_vertexCount - 4], _vertices[_vertexCount - 1], _vertices[_vertexCount - 3]);
			}

			AddTriangles();
		}

		private void GenerateFrontSide(int x, int y, int z, Color32 color)
		{
			_vertices[_vertexCount] = new VertexData
			{
				Position = new Vector3(x, y, z + 1), Normal = Vector3.forward, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, 0, 0, 1),
				UV = new Vector2(1, 0)
			};
			_vertices[_vertexCount + 1] = new VertexData
			{
				Position = new Vector3(x + 1, y, z + 1), Normal = Vector3.forward, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, 1, 0, 1),
				UV = new Vector2(0, 0)
			};
			_vertices[_vertexCount + 2] = new VertexData
			{
				Position = new Vector3(x, y + 1, z + 1), Normal = Vector3.forward, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, 0, 1, 1),
				UV = new Vector2(1, 1)
			};
			_vertices[_vertexCount + 3] = new VertexData
			{
				Position = new Vector3(x + 1, y + 1, z + 1), Normal = Vector3.forward, Red = color.r, Green = color.g, Blue = color.b,
				AmbientOcclusion = GetVertexAO(x, y, z, 1, 1, 1),
				UV = new Vector2(0, 1)
			};

			_vertexCount += 4;

			if (_vertices[_vertexCount - 4].AmbientOcclusion + _vertices[_vertexCount - 1].AmbientOcclusion >
			    _vertices[_vertexCount - 3].AmbientOcclusion + _vertices[_vertexCount - 2].AmbientOcclusion)
			{
				(_vertices[_vertexCount - 4], _vertices[_vertexCount - 3], _vertices[_vertexCount - 2], _vertices[_vertexCount - 1]) =
					(_vertices[_vertexCount - 2], _vertices[_vertexCount - 4], _vertices[_vertexCount - 1], _vertices[_vertexCount - 3]);
			}

			AddTriangles();
		}

		private void GenerateBackSide(int x, int y, int z, Color32 color)
		{
			_vertices[_vertexCount] = new VertexData
			{
				Position = new Vector3(x, y, z), Normal = Vector3.back, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, 0, 0, 0),
				UV = new Vector2(1, 1)
			};
			_vertices[_vertexCount + 1] = new VertexData
			{
				Position = new Vector3(x, y + 1, z), Normal = Vector3.back, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, 0, 1, 0),
				UV = new Vector2(1, 0)
			};
			_vertices[_vertexCount + 2] = new VertexData
			{
				Position = new Vector3(x + 1, y, z), Normal = Vector3.back, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, 1, 0, 0),
				UV = new Vector2(0, 1)
			};
			_vertices[_vertexCount + 3] = new VertexData
			{
				Position = new Vector3(x + 1, y + 1, z), Normal = Vector3.back, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, 1, 1, 0),
				UV = new Vector2(0, 0)
			};

			_vertexCount += 4;

			if (_vertices[_vertexCount - 4].AmbientOcclusion + _vertices[_vertexCount - 1].AmbientOcclusion >
			    _vertices[_vertexCount - 3].AmbientOcclusion + _vertices[_vertexCount - 2].AmbientOcclusion)
			{
				(_vertices[_vertexCount - 4], _vertices[_vertexCount - 3], _vertices[_vertexCount - 2], _vertices[_vertexCount - 1]) =
					(_vertices[_vertexCount - 2], _vertices[_vertexCount - 4], _vertices[_vertexCount - 1], _vertices[_vertexCount - 3]);
			}

			AddTriangles();
		}

		private void GenerateRightSide(int x, int y, int z, Color32 color)
		{
			_vertices[_vertexCount] = new VertexData
			{
				Position = new Vector3(x + 1, y, z), Normal = Vector3.right, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, 1, 0, 0),
				UV = new Vector2(0, 0)
			};
			_vertices[_vertexCount + 1] = new VertexData
			{
				Position = new Vector3(x + 1, y + 1, z), Normal = Vector3.right, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, 1, 1, 0),
				UV = new Vector2(0, 1)
			};
			_vertices[_vertexCount + 2] = new VertexData
			{
				Position = new Vector3(x + 1, y, z + 1), Normal = Vector3.right, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, 1, 0, 1),
				UV = new Vector2(1, 0)
			};
			_vertices[_vertexCount + 3] = new VertexData
			{
				Position = new Vector3(x + 1, y + 1, z + 1), Normal = Vector3.right, Red = color.r, Green = color.g, Blue = color.b,
				AmbientOcclusion = GetVertexAO(x, y, z, 1, 1, 1),
				UV = new Vector2(1, 1)
			};

			_vertexCount += 4;

			if (_vertices[_vertexCount - 4].AmbientOcclusion + _vertices[_vertexCount - 1].AmbientOcclusion >
			    _vertices[_vertexCount - 3].AmbientOcclusion
			    + _vertices[_vertexCount - 2].AmbientOcclusion)
			{
				(_vertices[_vertexCount - 4], _vertices[_vertexCount - 3], _vertices[_vertexCount - 2], _vertices[_vertexCount - 1]) =
					(_vertices[_vertexCount - 2], _vertices[_vertexCount - 4], _vertices[_vertexCount - 1], _vertices[_vertexCount - 3]);
			}

			AddTriangles();
		}

		private void GenerateLeftSide(int x, int y, int z, Color32 color)
		{
			_vertices[_vertexCount] = new VertexData
			{
				Position = new Vector3(x, y, z), Normal = Vector3.left, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, 0, 0, 0),
				UV = new Vector2(1, 0)
			};
			_vertices[_vertexCount + 1] = new VertexData
			{
				Position = new Vector3(x, y, z + 1), Normal = Vector3.left, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, 0, 0, 1),
				UV = new Vector2(0, 0)
			};
			_vertices[_vertexCount + 2] = new VertexData
			{
				Position = new Vector3(x, y + 1, z), Normal = Vector3.left, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, 0, 1, 0),
				UV = new Vector2(1, 1)
			};
			_vertices[_vertexCount + 3] = new VertexData
			{
				Position = new Vector3(x, y + 1, z + 1), Normal = Vector3.left, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, 0, 1, 1),
				UV = new Vector2(0, 1)
			};

			_vertexCount += 4;

			if (_vertices[_vertexCount - 4].AmbientOcclusion + _vertices[_vertexCount - 1].AmbientOcclusion
			    > _vertices[_vertexCount - 3].AmbientOcclusion + _vertices[_vertexCount - 2].AmbientOcclusion)
			{
				(_vertices[_vertexCount - 4], _vertices[_vertexCount - 3], _vertices[_vertexCount - 2], _vertices[_vertexCount - 1]) =
					(_vertices[_vertexCount - 2], _vertices[_vertexCount - 4], _vertices[_vertexCount - 1], _vertices[_vertexCount - 3]);
			}

			AddTriangles();
		}

		private void AddTriangles()
		{
			_indexes[6 * (_vertexCount - 4) / 4] = _vertexCount - 4;
			_indexes[6 * (_vertexCount - 4) / 4 + 1] = _vertexCount - 3;
			_indexes[6 * (_vertexCount - 4) / 4 + 2] = _vertexCount - 2;
			_indexes[6 * (_vertexCount - 4) / 4 + 3] = _vertexCount - 3;
			_indexes[6 * (_vertexCount - 4) / 4 + 4] = _vertexCount - 1;
			_indexes[6 * (_vertexCount - 4) / 4 + 5] = _vertexCount - 2;
		}

		private bool IsVisibleBlock(int x, int y, int z)
		{
			int index = _mapData.GetVoxelIndex(x, y, z);
			return _mapData.IsValidPosition(x, y, z) && _mapData[index].IsSolid();
		}

		private byte GetVertexAO(int x, int y, int z, int xOffset, int yOffset, int zOffset)
		{
			var xDirection = (int)Mathf.Sign(xOffset - Map.WorldOffset.x);
			var yDirection = (int)Mathf.Sign(yOffset - Map.WorldOffset.y);
			var zDirection = (int)Mathf.Sign(zOffset - Map.WorldOffset.z);
			int chunkOffsetX = _chunkIndex / (_mapData.Depth * _mapData.Height / Chunk.ChunkSizeSquared) * Chunk.ChunkSize;
			int chunkOffsetY = _chunkIndex / (_mapData.Depth / Chunk.ChunkSize) % (_mapData.Height / Chunk.ChunkSize) * Chunk.ChunkSize;
			int chunkOffsetZ = _chunkIndex % (_mapData.Depth / Chunk.ChunkSize) * Chunk.ChunkSize;
			var worldX = x + chunkOffsetX;
			var worldY = y + chunkOffsetY;
			var worldZ = z + chunkOffsetZ;
			bool side1 = IsVisibleBlock(worldX + xDirection, worldY + yDirection, worldZ);
			bool side2 = IsVisibleBlock(worldX, worldY + yDirection, worldZ + zDirection);
			bool corner = IsVisibleBlock(worldX + xDirection, worldY + yDirection, worldZ + zDirection);

			if (side1 && side2)
			{
				return 0;
			}

			return (byte)(3 - (BoolToByte(side1) + BoolToByte(side2) + BoolToByte(corner)));
		}

		private byte BoolToByte(bool value)
		{
			return value ? (byte)1 : (byte)0;
		}
	}
}
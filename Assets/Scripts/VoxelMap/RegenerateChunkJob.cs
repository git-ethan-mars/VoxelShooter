using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
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

		private readonly int _chunkOffsetX;
		private readonly int _chunkOffsetY;
		private readonly int _chunkOffsetZ;

		public RegenerateChunkJob(MapData mapData, int chunkIndex, NativeArray<VertexData> vertices, NativeArray<int> indexes)
		{
			_mapData = mapData;
			_chunkIndex = chunkIndex;
			_vertices = vertices;
			_indexes = indexes;
			_vertexCount = 0;
			_chunkOffsetX = _chunkIndex / (_mapData.Depth * _mapData.Height / Chunk.ChunkSizeSquared) * Chunk.ChunkSize;
			_chunkOffsetY = _chunkIndex / (_mapData.Depth / Chunk.ChunkSize) % (_mapData.Height / Chunk.ChunkSize) * Chunk.ChunkSize;
			_chunkOffsetZ = _chunkIndex % (_mapData.Depth / Chunk.ChunkSize) * Chunk.ChunkSize;

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
			float neighbours = UInt32ToFloat((uint)GetNeighbours(x, y, z, Face.Top));

			_vertices[_vertexCount] = new VertexData
			{
				Position = new Vector3(x, y + 1, z), Normal = Vector3.up, Red = color.r, Green = color.g, Blue =
					color.b,
				AmbientOcclusion = GetVertexAO(x, y, z, -1, 1, -1, Vector3.up),
				UV = new Vector2(0, 1), Neighbours = neighbours
			};
			_vertices[_vertexCount + 1] = new VertexData
			{
				Position = new Vector3(x, y + 1, z + 1), Normal = Vector3.up, Red = color.r, Green = color.g, Blue =
					color.b,
				AmbientOcclusion = GetVertexAO(x, y, z, -1, 1, 1, Vector3.up),
				UV = new Vector2(0, 0), Neighbours = neighbours
			};
			_vertices[_vertexCount + 2] = new VertexData
			{
				Position = new Vector3(x + 1, y + 1, z), Normal = Vector3.up, Red = color.r, Green = color.g, Blue =
					color.b,
				AmbientOcclusion = GetVertexAO(x, y, z, 1, 1, -1, Vector3.up),
				UV = new Vector2(1, 1), Neighbours = neighbours
			};
			_vertices[_vertexCount + 3] = new VertexData
			{
				Position = new Vector3(x + 1, y + 1, z + 1), Normal = Vector3.up, Red = color.r, Green = color.g, Blue
					= color.b,
				AmbientOcclusion = GetVertexAO(x, y, z, 1, 1, 1, Vector3.up),
				UV = new Vector2(1, 0), Neighbours = neighbours
			};

			_vertexCount += 4;

			SwapAmbientOcclusionIfNeeded();

			AddTriangles();
		}

		private void GenerateBottomSide(int x, int y, int z, Color32 color)
		{
			float neighbours = UInt32ToFloat((uint)GetNeighbours(x, y, z, Face.Bottom));

			_vertices[_vertexCount] = new VertexData
			{
				Position = new Vector3(x, y, z), Normal = Vector3.down, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, -1, -1, -1, Vector3.down),
				UV = new Vector2(1, 1), Neighbours = neighbours
			};
			_vertices[_vertexCount + 1] = new VertexData
			{
				Position = new Vector3(x + 1, y, z), Normal = Vector3.down, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, 1, -1, -1, Vector3.down),
				UV = new Vector2(0, 1), Neighbours = neighbours
			};
			_vertices[_vertexCount + 2] = new VertexData
			{
				Position = new Vector3(x, y, z + 1), Normal = Vector3.down, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, -1, -1, 1, Vector3.down),
				UV = new Vector2(1, 0), Neighbours = neighbours
			};
			_vertices[_vertexCount + 3] = new VertexData
			{
				Position = new Vector3(x + 1, y, z + 1), Normal = Vector3.down, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, 1, -1, 1, Vector3.down),
				UV = new Vector2(0, 0), Neighbours = neighbours
			};

			_vertexCount += 4;

			SwapAmbientOcclusionIfNeeded();

			AddTriangles();
		}

		private void GenerateFrontSide(int x, int y, int z, Color32 color)
		{
			float neighbours = UInt32ToFloat((uint)GetNeighbours(x, y, z, Face.Front));

			_vertices[_vertexCount] = new VertexData
			{
				Position = new Vector3(x, y, z + 1), Normal = Vector3.forward, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, -1, -1, 1, Vector3.forward),
				UV = new Vector2(1, 0), Neighbours = neighbours
			};
			_vertices[_vertexCount + 1] = new VertexData
			{
				Position = new Vector3(x + 1, y, z + 1), Normal = Vector3.forward, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, 1, -1, 1, Vector3.forward),
				UV = new Vector2(0, 0), Neighbours = neighbours
			};
			_vertices[_vertexCount + 2] = new VertexData
			{
				Position = new Vector3(x, y + 1, z + 1), Normal = Vector3.forward, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, -1, 1, 1, Vector3.forward),
				UV = new Vector2(1, 1), Neighbours = neighbours
			};
			_vertices[_vertexCount + 3] = new VertexData
			{
				Position = new Vector3(x + 1, y + 1, z + 1), Normal = Vector3.forward, Red = color.r, Green = color.g, Blue = color.b,
				AmbientOcclusion = GetVertexAO(x, y, z, 1, 1, 1, Vector3.forward),
				UV = new Vector2(0, 1), Neighbours = neighbours
			};

			_vertexCount += 4;

			SwapAmbientOcclusionIfNeeded();

			AddTriangles();
		}

		private void GenerateBackSide(int x, int y, int z, Color32 color)
		{
			float neighbours = UInt32ToFloat((uint)GetNeighbours(x, y, z, Face.Back));

			_vertices[_vertexCount] = new VertexData
			{
				Position = new Vector3(x, y, z), Normal = Vector3.back, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, -1, -1, -1, Vector3.back),
				UV = new Vector2(0, 0), Neighbours = neighbours
			};
			_vertices[_vertexCount + 1] = new VertexData
			{
				Position = new Vector3(x, y + 1, z), Normal = Vector3.back, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, -1, 1, -1, Vector3.back),
				UV = new Vector2(0, 1), Neighbours = neighbours
			};
			_vertices[_vertexCount + 2] = new VertexData
			{
				Position = new Vector3(x + 1, y, z), Normal = Vector3.back, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, 1, -1, -1, Vector3.back),
				UV = new Vector2(1, 0), Neighbours = neighbours
			};
			_vertices[_vertexCount + 3] = new VertexData
			{
				Position = new Vector3(x + 1, y + 1, z), Normal = Vector3.back, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, 1, 1, -1, Vector3.back),
				UV = new Vector2(1, 1), Neighbours = neighbours
			};

			_vertexCount += 4;

			SwapAmbientOcclusionIfNeeded();

			AddTriangles();
		}

		private void GenerateRightSide(int x, int y, int z, Color32 color)
		{
			float neighbours = UInt32ToFloat((uint)GetNeighbours(x, y, z, Face.Right));

			_vertices[_vertexCount] = new VertexData
			{
				Position = new Vector3(x + 1, y, z), Normal = Vector3.right, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, 1, -1, -1, Vector3.right),
				UV = new Vector2(0, 0), Neighbours = neighbours
			};
			_vertices[_vertexCount + 1] = new VertexData
			{
				Position = new Vector3(x + 1, y + 1, z), Normal = Vector3.right, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, 1, 1, -1, Vector3.right),
				UV = new Vector2(0, 1), Neighbours = neighbours
			};
			_vertices[_vertexCount + 2] = new VertexData
			{
				Position = new Vector3(x + 1, y, z + 1), Normal = Vector3.right, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, 1, -1, 1, Vector3.right),
				UV = new Vector2(1, 0), Neighbours = neighbours
			};
			_vertices[_vertexCount + 3] = new VertexData
			{
				Position = new Vector3(x + 1, y + 1, z + 1), Normal = Vector3.right, Red = color.r, Green = color.g, Blue = color.b,
				AmbientOcclusion = GetVertexAO(x, y, z, 1, 1, 1, Vector3.right),
				UV = new Vector2(1, 1), Neighbours = neighbours
			};

			_vertexCount += 4;

			SwapAmbientOcclusionIfNeeded();

			AddTriangles();
		}

		private void GenerateLeftSide(int x, int y, int z, Color32 color)
		{
			float neighbours = UInt32ToFloat((uint)GetNeighbours(x, y, z, Face.Left));

			_vertices[_vertexCount] = new VertexData
			{
				Position = new Vector3(x, y, z), Normal = Vector3.left, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, -1, -1, -1, Vector3.left),
				UV = new Vector2(1, 0), Neighbours = neighbours
			};
			_vertices[_vertexCount + 1] = new VertexData
			{
				Position = new Vector3(x, y, z + 1), Normal = Vector3.left, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, -1, -1, 1, Vector3.left),
				UV = new Vector2(0, 0), Neighbours = neighbours
			};
			_vertices[_vertexCount + 2] = new VertexData
			{
				Position = new Vector3(x, y + 1, z), Normal = Vector3.left, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, -1, 1, -1, Vector3.left),
				UV = new Vector2(1, 1), Neighbours = neighbours
			};
			_vertices[_vertexCount + 3] = new VertexData
			{
				Position = new Vector3(x, y + 1, z + 1), Normal = Vector3.left, Red = color.r, Green = color.g, Blue = color.b, AmbientOcclusion =
					GetVertexAO(x, y, z, -1, 1, 1, Vector3.left),
				UV = new Vector2(0, 1), Neighbours = neighbours
			};

			_vertexCount += 4;

			SwapAmbientOcclusionIfNeeded();

			AddTriangles();
		}

		private void SwapAmbientOcclusionIfNeeded()
		{
			if (_vertices[_vertexCount - 4].AmbientOcclusion + _vertices[_vertexCount - 1].AmbientOcclusion >
			    _vertices[_vertexCount - 3].AmbientOcclusion + _vertices[_vertexCount - 2].AmbientOcclusion)
			{
				(_vertices[_vertexCount - 4], _vertices[_vertexCount - 3], _vertices[_vertexCount - 2], _vertices[_vertexCount - 1]) =
					(_vertices[_vertexCount - 2], _vertices[_vertexCount - 4], _vertices[_vertexCount - 1], _vertices[_vertexCount - 3]);
			}
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
			return _mapData.IsValidPosition(x, y, z) && _mapData[x, y, z].IsSolid();
		}

		private byte GetVertexAO(int x, int y, int z, int xOffset, int yOffset, int zOffset, Vector3 normal)
		{
			int worldX = x + _chunkOffsetX;
			int worldY = y + _chunkOffsetY;
			int worldZ = z + _chunkOffsetZ;

			bool side1;
			bool side2;

			if (normal == Vector3.up || normal == Vector3.down)
			{
				side1 = IsVisibleBlock(worldX, worldY + yOffset, worldZ + zOffset);
				side2 = IsVisibleBlock(worldX + xOffset, worldY + yOffset, worldZ);
			}
			else if (normal == Vector3.right || normal == Vector3.left)
			{
				side1 = IsVisibleBlock(worldX + xOffset, worldY, worldZ + zOffset);
				side2 = IsVisibleBlock(worldX + xOffset, worldY + yOffset, worldZ);
			}
			else
			{
				side1 = IsVisibleBlock(worldX, worldY + yOffset, worldZ + zOffset);
				side2 = IsVisibleBlock(worldX + xOffset, worldY, worldZ + zOffset);
			}

			bool corner = IsVisibleBlock(worldX + xOffset, worldY + yOffset, worldZ + zOffset);


			if (side1 && side2)
			{
				return 0;
			}

			return (byte)(3 - (Convert.ToByte(side1) + Convert.ToByte(side2) + Convert.ToByte(corner)));
		}

		private int GetNeighbours(int x, int y, int z, Face face)
		{
			int worldX = x + _chunkOffsetX;
			int worldY = y + _chunkOffsetY;
			int worldZ = z + _chunkOffsetZ;
			int neighbours = 0;

			if (face == Face.Top || face == Face.Bottom)
			{
				// Плоскость: XZ
				// U = X, V = -Z  → uv.x = X, uv.y = 1 - Z
				// right = +X, left = -X, front = -Z, back = +Z
				bool isTop = face == Face.Top;
				int rightX = isTop ? +1 : -1;
				int leftX = isTop ? -1 : +1;

				// va: right, left, front, back
				neighbours |= (IsVisibleBlock(worldX + rightX, worldY, worldZ) ? 1 : 0) << 0; // right
				neighbours |= (IsVisibleBlock(worldX + leftX, worldY, worldZ) ? 1 : 0) << 1;  // left
				neighbours |= (IsVisibleBlock(worldX, worldY, worldZ - 1) ? 1 : 0) << 2;      // front (-Z)
				neighbours |= (IsVisibleBlock(worldX, worldY, worldZ + 1) ? 1 : 0) << 3;      // back  (+Z)
				
				
				// vb: front-right, front-left, back-left, back-right
				neighbours |= (IsVisibleBlock(worldX + rightX, worldY, worldZ - 1) ? 1 : 0) << 4; // fr
				neighbours |= (IsVisibleBlock(worldX + leftX, worldY, worldZ - 1) ? 1 : 0) << 5;  // fl
				neighbours |= (IsVisibleBlock(worldX + leftX, worldY, worldZ + 1) ? 1 : 0) << 6;  // bl
				neighbours |= (IsVisibleBlock(worldX + rightX, worldY, worldZ + 1) ? 1 : 0) << 7; // br

				int upwardY = isTop ? +1 : -1;
				
				// vc: right, left, front, back (на уровне выше: Y+1)
				neighbours |= (IsVisibleBlock(worldX + rightX, worldY + upwardY, worldZ) ? 1 : 0) << 8;
				neighbours |= (IsVisibleBlock(worldX + leftX, worldY + upwardY, worldZ) ? 1 : 0) << 9;
				neighbours |= (IsVisibleBlock(worldX, worldY + upwardY, worldZ - 1) ? 1 : 0) << 10;
				neighbours |= (IsVisibleBlock(worldX, worldY + upwardY, worldZ + 1) ? 1 : 0) << 11;

				// vd: front-right, front-left, back-left, back-right (Y+1)
				neighbours |= (IsVisibleBlock(worldX + rightX, worldY + upwardY, worldZ - 1) ? 1 : 0) << 12;
				neighbours |= (IsVisibleBlock(worldX + leftX, worldY + upwardY, worldZ - 1) ? 1 : 0) << 13;
				neighbours |= (IsVisibleBlock(worldX + leftX, worldY + upwardY, worldZ + 1) ? 1 : 0) << 14;
				neighbours |= (IsVisibleBlock(worldX + rightX, worldY + upwardY, worldZ + 1) ? 1 : 0) << 15;
			}
			else if (face == Face.Front || face == Face.Back)
			{
				// Плоскость: XY
				// Для front (+Z): U = X, V = Y → uv.x = X, uv.y = Y
				// Для back  (-Z): чтобы (0,0) был внизу слева, нужно инвертировать X
				bool isFront = face == Face.Front;

				int rightX = isFront ? -1 : +1;
				int leftX = isFront ? +1 : -1;

				// va: right, left, front, back → в плоскости XY: front = +Y, back = -Y
				neighbours |= (IsVisibleBlock(worldX + rightX, worldY, worldZ) ? 1 : 0) << 0; // right
				neighbours |= (IsVisibleBlock(worldX + leftX, worldY, worldZ) ? 1 : 0) << 1;  // left
				neighbours |= (IsVisibleBlock(worldX, worldY + 1, worldZ) ? 1 : 0) << 2;      // front (+Y)
				neighbours |= (IsVisibleBlock(worldX, worldY - 1, worldZ) ? 1 : 0) << 3;      // back  (-Y)

				// vb: front-right, front-left, back-left, back-right
				neighbours |= (IsVisibleBlock(worldX + rightX, worldY + 1, worldZ) ? 1 : 0) << 4; // fr
				neighbours |= (IsVisibleBlock(worldX + leftX, worldY + 1, worldZ) ? 1 : 0) << 5;  // fl
				neighbours |= (IsVisibleBlock(worldX + leftX, worldY - 1, worldZ) ? 1 : 0) << 6;  // bl
				neighbours |= (IsVisibleBlock(worldX + rightX, worldY - 1, worldZ) ? 1 : 0) << 7; // br

				// vc: right, left, front, back (на уровне "вперёд": Z+1 для front, Z-1 для back)
				int forwardZ = isFront ? +1 : -1;

				neighbours |= (IsVisibleBlock(worldX + rightX, worldY, worldZ + forwardZ) ? 1 : 0) << 8;
				neighbours |= (IsVisibleBlock(worldX + leftX, worldY, worldZ + forwardZ) ? 1 : 0) << 9;
				neighbours |= (IsVisibleBlock(worldX, worldY + 1, worldZ + forwardZ) ? 1 : 0) << 10;
				neighbours |= (IsVisibleBlock(worldX, worldY - 1, worldZ + forwardZ) ? 1 : 0) << 11;

				// vd: углы "вперёд"
				neighbours |= (IsVisibleBlock(worldX + rightX, worldY + 1, worldZ + forwardZ) ? 1 : 0) << 12;
				neighbours |= (IsVisibleBlock(worldX + leftX, worldY + 1, worldZ + forwardZ) ? 1 : 0) << 13;
				neighbours |= (IsVisibleBlock(worldX + leftX, worldY - 1, worldZ + forwardZ) ? 1 : 0) << 14;
				neighbours |= (IsVisibleBlock(worldX + rightX, worldY - 1, worldZ + forwardZ) ? 1 : 0) << 15;
			}
			else if (face == Face.Right || face == Face.Left)
			{
				bool isRight = face == Face.Right;

				int rightZ = isRight ? +1 : -1; // направление "right" на грани
				int leftZ = isRight ? -1 : +1;

				// va: right, left, front, back → front = +Y, back = -Y
				neighbours |= (IsVisibleBlock(worldX, worldY, worldZ + rightZ) ? 1 : 0) << 0; // right
				neighbours |= (IsVisibleBlock(worldX, worldY, worldZ + leftZ) ? 1 : 0) << 1;  // left
				neighbours |= (IsVisibleBlock(worldX, worldY + 1, worldZ) ? 1 : 0) << 2;      // front (+Y)
				neighbours |= (IsVisibleBlock(worldX, worldY - 1, worldZ) ? 1 : 0) << 3;      // back  (-Y)

				// vb: front-right, front-left, back-left, back-right
				neighbours |= (IsVisibleBlock(worldX, worldY + 1, worldZ + rightZ) ? 1 : 0) << 4; // fr
				neighbours |= (IsVisibleBlock(worldX, worldY + 1, worldZ + leftZ) ? 1 : 0) << 5;  // fl
				neighbours |= (IsVisibleBlock(worldX, worldY - 1, worldZ + leftZ) ? 1 : 0) << 6;  // bl
				neighbours |= (IsVisibleBlock(worldX, worldY - 1, worldZ + rightZ) ? 1 : 0) << 7; // br

				// vc: right, left, front, back (на уровне "вправо": X+1 для right, X-1 для left)
				int outwardX = isRight ? +1 : -1;

				neighbours |= (IsVisibleBlock(worldX + outwardX, worldY, worldZ + rightZ) ? 1 : 0) << 8;
				neighbours |= (IsVisibleBlock(worldX + outwardX, worldY, worldZ + leftZ) ? 1 : 0) << 9;
				neighbours |= (IsVisibleBlock(worldX + outwardX, worldY + 1, worldZ) ? 1 : 0) << 10;
				neighbours |= (IsVisibleBlock(worldX + outwardX, worldY - 1, worldZ) ? 1 : 0) << 11;

				// vd: углы "вправо"
				neighbours |= (IsVisibleBlock(worldX + outwardX, worldY + 1, worldZ + rightZ) ? 1 : 0) << 12;
				neighbours |= (IsVisibleBlock(worldX + outwardX, worldY + 1, worldZ + leftZ) ? 1 : 0) << 13;
				neighbours |= (IsVisibleBlock(worldX + outwardX, worldY - 1, worldZ + leftZ) ? 1 : 0) << 14;
				neighbours |= (IsVisibleBlock(worldX + outwardX, worldY - 1, worldZ + rightZ) ? 1 : 0) << 15;
			}

			return neighbours;
		}

		private unsafe float UInt32ToFloat(uint value)
		{
			return *(float*)&value;
		}
	}
}
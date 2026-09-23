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
	public struct FillChunkMeshJob : IJob
	{
		private MapData _mapData;
		private readonly int _chunkIndex;
		[NativeDisableContainerSafetyRestriction]
		private NativeArray<VertexData> _chunkVertices;
		[NativeDisableContainerSafetyRestriction]
		private NativeArray<int> _chunkIndexes;
		private int _vertexCount;

		private readonly ushort _chunkOffsetX;
		private readonly ushort _chunkOffsetY;
		private readonly ushort _chunkOffsetZ;

		public FillChunkMeshJob(MapData mapData, int chunkIndex, NativeArray<VertexData> chunkVertices, NativeArray<int> chunkChunkIndexes)
		{
			_mapData = mapData;
			_chunkIndex = chunkIndex;
			_chunkVertices = chunkVertices;
			_chunkIndexes = chunkChunkIndexes;
			_vertexCount = 0;
			_chunkOffsetX = (ushort)(_chunkIndex / (_mapData.Depth * _mapData.Height / Chunk.ChunkSizeSquared) * Chunk.ChunkSize);
			_chunkOffsetY = (ushort)(_chunkIndex / (_mapData.Depth / Chunk.ChunkSize) % (_mapData.Height / Chunk.ChunkSize) * Chunk.ChunkSize);
			_chunkOffsetZ = (ushort)(_chunkIndex % (_mapData.Depth / Chunk.ChunkSize) * Chunk.ChunkSize);
		}

		public void Execute()
		{
			for (var i = 0; i < Chunk.ChunkSizeCubed; i++)
			{
				var z = (ushort)(i % Chunk.ChunkSize);
				var y = (ushort)(i / Chunk.ChunkSize % Chunk.ChunkSize);
				var x = (ushort)(i / Chunk.ChunkSizeSquared);
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

		private void GenerateVoxel(ushort x, ushort y, ushort z, Face faces, Color32 color)
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

		private void GenerateTopSide(ushort x, ushort y, ushort z, Color32 color)
		{
			float neighbours = UInt32ToFloat((uint)GetNeighbours(x, y, z, Face.Top));

			_chunkVertices[_vertexCount] = new VertexData
			{
				Position = new Vector3(x, y + 1, z), Normal = Vector3.up,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, -1, 1, -1, Vector3.up) << 24),
				UV = new Vector2(0, 1), Neighbours = neighbours
			};
			_chunkVertices[_vertexCount + 1] = new VertexData
			{
				Position = new Vector3(x, y + 1, z + 1), Normal = Vector3.up,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, -1, 1, 1, Vector3.up) << 24),
				UV = new Vector2(0, 0), Neighbours = neighbours
			};
			_chunkVertices[_vertexCount + 2] = new VertexData
			{
				Position = new Vector3(x + 1, y + 1, z), Normal = Vector3.up,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, 1, 1, -1, Vector3.up) << 24),
				UV = new Vector2(1, 1), Neighbours = neighbours
			};
			_chunkVertices[_vertexCount + 3] = new VertexData
			{
				Position = new Vector3(x + 1, y + 1, z + 1), Normal = Vector3.up,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, 1, 1, 1, Vector3.up) << 24),
				UV = new Vector2(1, 0), Neighbours = neighbours
			};

			_vertexCount += 4;

			SwapAmbientOcclusionIfNeeded();

			AddTriangles();
		}

		private void GenerateBottomSide(ushort x, ushort y, ushort z, Color32 color)
		{
			float neighbours = UInt32ToFloat((uint)GetNeighbours(x, y, z, Face.Bottom));

			_chunkVertices[_vertexCount] = new VertexData
			{
				Position = new Vector3(x, y, z), Normal = Vector3.down,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, -1, -1, -1, Vector3.down) << 24),
				UV = new Vector2(1, 1), Neighbours = neighbours
			};
			_chunkVertices[_vertexCount + 1] = new VertexData
			{
				Position = new Vector3(x + 1, y, z), Normal = Vector3.down,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, 1, -1, -1, Vector3.down) << 24),
				UV = new Vector2(0, 1), Neighbours = neighbours
			};
			_chunkVertices[_vertexCount + 2] = new VertexData
			{
				Position = new Vector3(x, y, z + 1), Normal = Vector3.down,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, -1, -1, 1, Vector3.down) << 24),
				UV = new Vector2(1, 0), Neighbours = neighbours
			};
			_chunkVertices[_vertexCount + 3] = new VertexData
			{
				Position = new Vector3(x + 1, y, z + 1), Normal = Vector3.down,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, 1, -1, 1, Vector3.down) << 24),
				UV = new Vector2(0, 0), Neighbours = neighbours
			};

			_vertexCount += 4;

			SwapAmbientOcclusionIfNeeded();

			AddTriangles();
		}

		private void GenerateFrontSide(ushort x, ushort y, ushort z, Color32 color)
		{
			float neighbours = UInt32ToFloat((uint)GetNeighbours(x, y, z, Face.Front));

			_chunkVertices[_vertexCount] = new VertexData
			{
				Position = new Vector3(x, y, z + 1), Normal = Vector3.forward,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, -1, -1, 1, Vector3.forward) << 24),
				UV = new Vector2(1, 0), Neighbours = neighbours
			};
			_chunkVertices[_vertexCount + 1] = new VertexData
			{
				Position = new Vector3(x + 1, y, z + 1), Normal = Vector3.forward,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, 1, -1, 1, Vector3.forward) << 24),
				UV = new Vector2(0, 0), Neighbours = neighbours
			};
			_chunkVertices[_vertexCount + 2] = new VertexData
			{
				Position = new Vector3(x, y + 1, z + 1), Normal = Vector3.forward,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, -1, 1, 1, Vector3.forward) << 24),
				UV = new Vector2(1, 1), Neighbours = neighbours
			};
			_chunkVertices[_vertexCount + 3] = new VertexData
			{
				Position = new Vector3(x + 1, y + 1, z + 1), Normal = Vector3.forward,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, 1, 1, 1, Vector3.forward) << 24),
				UV = new Vector2(0, 1), Neighbours = neighbours
			};

			_vertexCount += 4;

			SwapAmbientOcclusionIfNeeded();

			AddTriangles();
		}

		private void GenerateBackSide(ushort x, ushort y, ushort z, Color32 color)
		{
			float neighbours = UInt32ToFloat((uint)GetNeighbours(x, y, z, Face.Back));

			_chunkVertices[_vertexCount] = new VertexData
			{
				Position = new Vector3(x, y, z), Normal = Vector3.back,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, -1, -1, -1, Vector3.back) << 24),
				UV = new Vector2(0, 0), Neighbours = neighbours
			};
			_chunkVertices[_vertexCount + 1] = new VertexData
			{
				Position = new Vector3(x, y + 1, z), Normal = Vector3.back,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, -1, 1, -1, Vector3.back) << 24),
				UV = new Vector2(0, 1), Neighbours = neighbours
			};
			_chunkVertices[_vertexCount + 2] = new VertexData
			{
				Position = new Vector3(x + 1, y, z), Normal = Vector3.back,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, 1, -1, -1, Vector3.back) << 24),
				UV = new Vector2(1, 0), Neighbours = neighbours
			};
			_chunkVertices[_vertexCount + 3] = new VertexData
			{
				Position = new Vector3(x + 1, y + 1, z), Normal = Vector3.back,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, 1, 1, -1, Vector3.back) << 24),
				UV = new Vector2(1, 1), Neighbours = neighbours
			};

			_vertexCount += 4;

			SwapAmbientOcclusionIfNeeded();

			AddTriangles();
		}

		private void GenerateRightSide(ushort x, ushort y, ushort z, Color32 color)
		{
			float neighbours = UInt32ToFloat((uint)GetNeighbours(x, y, z, Face.Right));

			_chunkVertices[_vertexCount] = new VertexData
			{
				Position = new Vector3(x + 1, y, z), Normal = Vector3.right,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, 1, -1, -1, Vector3.right) << 24),
				UV = new Vector2(0, 0), Neighbours = neighbours
			};
			_chunkVertices[_vertexCount + 1] = new VertexData
			{
				Position = new Vector3(x + 1, y + 1, z), Normal = Vector3.right,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, 1, 1, -1, Vector3.right) << 24),
				UV = new Vector2(0, 1), Neighbours = neighbours
			};
			_chunkVertices[_vertexCount + 2] = new VertexData
			{
				Position = new Vector3(x + 1, y, z + 1), Normal = Vector3.right,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, 1, -1, 1, Vector3.right) << 24),
				UV = new Vector2(1, 0), Neighbours = neighbours
			};
			_chunkVertices[_vertexCount + 3] = new VertexData
			{
				Position = new Vector3(x + 1, y + 1, z + 1), Normal = Vector3.right,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, 1, 1, 1, Vector3.right) << 24),
				UV = new Vector2(1, 1), Neighbours = neighbours
			};

			_vertexCount += 4;

			SwapAmbientOcclusionIfNeeded();

			AddTriangles();
		}

		private void GenerateLeftSide(ushort x, ushort y, ushort z, Color32 color)
		{
			float neighbours = UInt32ToFloat((uint)GetNeighbours(x, y, z, Face.Left));

			_chunkVertices[_vertexCount] = new VertexData
			{
				Position = new Vector3(x, y, z), Normal = Vector3.left,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, -1, -1, -1, Vector3.left) << 24),
				UV = new Vector2(1, 0), Neighbours = neighbours
			};
			_chunkVertices[_vertexCount + 1] = new VertexData
			{
				Position = new Vector3(x, y, z + 1), Normal = Vector3.left,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, -1, -1, 1, Vector3.left) << 24),
				UV = new Vector2(0, 0), Neighbours = neighbours
			};
			_chunkVertices[_vertexCount + 2] = new VertexData
			{
				Position = new Vector3(x, y + 1, z), Normal = Vector3.left,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, -1, 1, -1, Vector3.left) << 24),
				UV = new Vector2(1, 1), Neighbours = neighbours
			};
			_chunkVertices[_vertexCount + 3] = new VertexData
			{
				Position = new Vector3(x, y + 1, z + 1), Normal = Vector3.left,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, -1, 1, 1, Vector3.left) << 24),
				UV = new Vector2(0, 1), Neighbours = neighbours
			};

			_vertexCount += 4;

			SwapAmbientOcclusionIfNeeded();

			AddTriangles();
		}

		private void SwapAmbientOcclusionIfNeeded()
		{
			if (_chunkVertices[_vertexCount - 4].AO + _chunkVertices[_vertexCount - 1].AO >
			    _chunkVertices[_vertexCount - 3].AO + _chunkVertices[_vertexCount - 2].AO)
			{
				(_chunkVertices[_vertexCount - 4], _chunkVertices[_vertexCount - 3], _chunkVertices[_vertexCount - 2],
						_chunkVertices[_vertexCount - 1]) =
					(_chunkVertices[_vertexCount - 2], _chunkVertices[_vertexCount - 4], _chunkVertices[_vertexCount - 1],
						_chunkVertices[_vertexCount - 3]);
			}
		}

		private void AddTriangles()
		{
			_chunkIndexes[6 * (_vertexCount - 4) / 4] = _vertexCount - 4;
			_chunkIndexes[6 * (_vertexCount - 4) / 4 + 1] = _vertexCount - 3;
			_chunkIndexes[6 * (_vertexCount - 4) / 4 + 2] = _vertexCount - 2;
			_chunkIndexes[6 * (_vertexCount - 4) / 4 + 3] = _vertexCount - 3;
			_chunkIndexes[6 * (_vertexCount - 4) / 4 + 4] = _vertexCount - 1;
			_chunkIndexes[6 * (_vertexCount - 4) / 4 + 5] = _vertexCount - 2;
		}

		private bool IsVisibleBlock(ushort x, ushort y, ushort z)
		{
			return _mapData.IsValidPosition(x, y, z) && _mapData[x, y, z].IsSolid();
		}

		private byte GetVertexAO(int x, int y, int z, int xOffset, int yOffset, int zOffset, Vector3 normal)
		{
			var worldX = (ushort)(x + _chunkOffsetX);
			var worldY = (ushort)(y + _chunkOffsetY);
			var worldZ = (ushort)(z + _chunkOffsetZ);

			bool side1;
			bool side2;

			if (normal == Vector3.up || normal == Vector3.down)
			{
				side1 = IsVisibleBlock(worldX, (ushort)(worldY + yOffset), (ushort)(worldZ + zOffset));
				side2 = IsVisibleBlock((ushort)(worldX + xOffset), (ushort)(worldY + yOffset), worldZ);
			}
			else if (normal == Vector3.right || normal == Vector3.left)
			{
				side1 = IsVisibleBlock((ushort)(worldX + xOffset), worldY, (ushort)(worldZ + zOffset));
				side2 = IsVisibleBlock((ushort)(worldX + xOffset), (ushort)(worldY + yOffset), worldZ);
			}
			else
			{
				side1 = IsVisibleBlock(worldX, (ushort)(worldY + yOffset), (ushort)(worldZ + zOffset));
				side2 = IsVisibleBlock((ushort)(worldX + xOffset), worldY, (ushort)(worldZ + zOffset));
			}

			bool corner = IsVisibleBlock((ushort)(worldX + xOffset), (ushort)(worldY + yOffset), (ushort)(worldZ + zOffset));


			if (side1 && side2)
			{
				return 0;
			}

			return (byte)(3 - (Convert.ToByte(side1) + Convert.ToByte(side2) + Convert.ToByte(corner)));
		}

		private int GetNeighbours(ushort x, ushort y, ushort z, Face face)
		{
			var worldX = (ushort)(x + _chunkOffsetX);
			var worldY = (ushort)(y + _chunkOffsetY);
			var worldZ = (ushort)(z + _chunkOffsetZ);
			var neighbours = 0;

			if (face == Face.Top || face == Face.Bottom)
			{
				// Плоскость: XZ
				// U = X, V = -Z  → uv.x = X, uv.y = 1 - Z
				// right = +X, left = -X, front = -Z, back = +Z
				bool isTop = face == Face.Top;
				int rightX = isTop ? +1 : -1;
				int leftX = isTop ? -1 : +1;

				// va: right, left, front, back
				neighbours |= (IsVisibleBlock((ushort)(worldX + rightX), worldY, worldZ) ? 1 : 0) << 0; // right
				neighbours |= (IsVisibleBlock((ushort)(worldX + leftX), worldY, worldZ) ? 1 : 0) << 1;  // left
				neighbours |= (IsVisibleBlock(worldX, worldY, (ushort)(worldZ - 1)) ? 1 : 0) << 2;      // front (-Z)
				neighbours |= (IsVisibleBlock(worldX, worldY, (ushort)(worldZ + 1)) ? 1 : 0) << 3;      // back  (+Z)


				// vb: front-right, front-left, back-left, back-right
				neighbours |= (IsVisibleBlock((ushort)(worldX + rightX), worldY, (ushort)(worldZ - 1)) ? 1 : 0) << 4; // fr
				neighbours |= (IsVisibleBlock((ushort)(worldX + leftX), worldY, (ushort)(worldZ - 1)) ? 1 : 0) << 5;  // fl
				neighbours |= (IsVisibleBlock((ushort)(worldX + leftX), worldY, (ushort)(worldZ + 1)) ? 1 : 0) << 6;  // bl
				neighbours |= (IsVisibleBlock((ushort)(worldX + rightX), worldY, (ushort)(worldZ + 1)) ? 1 : 0) << 7; // br

				int upwardY = isTop ? +1 : -1;

				// vc: right, left, front, back (на уровне выше: Y+1)
				neighbours |= (IsVisibleBlock((ushort)(worldX + rightX), (ushort)(worldY + upwardY), worldZ) ? 1 : 0) << 8;
				neighbours |= (IsVisibleBlock((ushort)(worldX + leftX), (ushort)(worldY + upwardY), worldZ) ? 1 : 0) << 9;
				neighbours |= (IsVisibleBlock(worldX, (ushort)(worldY + upwardY), (ushort)(worldZ - 1)) ? 1 : 0) << 10;
				neighbours |= (IsVisibleBlock(worldX, (ushort)(worldY + upwardY), (ushort)(worldZ + 1)) ? 1 : 0) << 11;

				// vd: front-right, front-left, back-left, back-right (Y+1)
				neighbours |= (IsVisibleBlock((ushort)(worldX + rightX), (ushort)(worldY + upwardY), (ushort)(worldZ - 1)) ? 1 : 0) << 12;
				neighbours |= (IsVisibleBlock((ushort)(worldX + leftX), (ushort)(worldY + upwardY), (ushort)(worldZ - 1)) ? 1 : 0) << 13;
				neighbours |= (IsVisibleBlock((ushort)(worldX + leftX), (ushort)(worldY + upwardY), (ushort)(worldZ + 1)) ? 1 : 0) << 14;
				neighbours |= (IsVisibleBlock((ushort)(worldX + rightX), (ushort)(worldY + upwardY), (ushort)(worldZ + 1)) ? 1 : 0) << 15;
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
				neighbours |= (IsVisibleBlock((ushort)(worldX + rightX), worldY, worldZ) ? 1 : 0) << 0; // right
				neighbours |= (IsVisibleBlock((ushort)(worldX + leftX), worldY, worldZ) ? 1 : 0) << 1;  // left
				neighbours |= (IsVisibleBlock(worldX, (ushort)(worldY + 1), worldZ) ? 1 : 0) << 2;      // front (+Y)
				neighbours |= (IsVisibleBlock(worldX, (ushort)(worldY - 1), worldZ) ? 1 : 0) << 3;      // back  (-Y)

				// vb: front-right, front-left, back-left, back-right
				neighbours |= (IsVisibleBlock((ushort)(worldX + rightX), (ushort)(worldY + 1), worldZ) ? 1 : 0) << 4; // fr
				neighbours |= (IsVisibleBlock((ushort)(worldX + leftX), (ushort)(worldY + 1), worldZ) ? 1 : 0) << 5;  // fl
				neighbours |= (IsVisibleBlock((ushort)(worldX + leftX), (ushort)(worldY - 1), worldZ) ? 1 : 0) << 6;  // bl
				neighbours |= (IsVisibleBlock((ushort)(worldX + rightX), (ushort)(worldY - 1), worldZ) ? 1 : 0) << 7; // br

				// vc: right, left, front, back (на уровне "вперёд": Z+1 для front, Z-1 для back)
				int forwardZ = isFront ? +1 : -1;

				neighbours |= (IsVisibleBlock((ushort)(worldX + rightX), worldY, (ushort)(worldZ + forwardZ)) ? 1 : 0) << 8;
				neighbours |= (IsVisibleBlock((ushort)(worldX + leftX), worldY, (ushort)(worldZ + forwardZ)) ? 1 : 0) << 9;
				neighbours |= (IsVisibleBlock(worldX, (ushort)(worldY + 1), (ushort)(worldZ + forwardZ)) ? 1 : 0) << 10;
				neighbours |= (IsVisibleBlock(worldX, (ushort)(worldY - 1), (ushort)(worldZ + forwardZ)) ? 1 : 0) << 11;

				// vd: углы "вперёд"
				neighbours |= (IsVisibleBlock((ushort)(worldX + rightX), (ushort)(worldY + 1), (ushort)(worldZ + forwardZ)) ? 1 : 0) << 12;
				neighbours |= (IsVisibleBlock((ushort)(worldX + leftX), (ushort)(worldY + 1), (ushort)(worldZ + forwardZ)) ? 1 : 0) << 13;
				neighbours |= (IsVisibleBlock((ushort)(worldX + leftX), (ushort)(worldY - 1), (ushort)(worldZ + forwardZ)) ? 1 : 0) << 14;
				neighbours |= (IsVisibleBlock((ushort)(worldX + rightX), (ushort)(worldY - 1), (ushort)(worldZ + forwardZ)) ? 1 : 0) << 15;
			}
			else if (face == Face.Right || face == Face.Left)
			{
				bool isRight = face == Face.Right;

				int rightZ = isRight ? +1 : -1; // направление "right" на грани
				int leftZ = isRight ? -1 : +1;

				// va: right, left, front, back → front = +Y, back = -Y
				neighbours |= (IsVisibleBlock(worldX, worldY, (ushort)(worldZ + rightZ)) ? 1 : 0) << 0; // right
				neighbours |= (IsVisibleBlock(worldX, worldY, (ushort)(worldZ + leftZ)) ? 1 : 0) << 1;  // left
				neighbours |= (IsVisibleBlock(worldX, (ushort)(worldY + 1), worldZ) ? 1 : 0) << 2;      // front (+Y)
				neighbours |= (IsVisibleBlock(worldX, (ushort)(worldY - 1), worldZ) ? 1 : 0) << 3;      // back  (-Y)

				// vb: front-right, front-left, back-left, back-right
				neighbours |= (IsVisibleBlock(worldX, (ushort)(worldY + 1), (ushort)(worldZ + rightZ)) ? 1 : 0) << 4; // fr
				neighbours |= (IsVisibleBlock(worldX, (ushort)(worldY + 1), (ushort)(worldZ + leftZ)) ? 1 : 0) << 5;  // fl
				neighbours |= (IsVisibleBlock(worldX, (ushort)(worldY - 1), (ushort)(worldZ + leftZ)) ? 1 : 0) << 6;  // bl
				neighbours |= (IsVisibleBlock(worldX, (ushort)(worldY - 1), (ushort)(worldZ + rightZ)) ? 1 : 0) << 7; // br

				// vc: right, left, front, back (на уровне "вправо": X+1 для right, X-1 для left)
				int outwardX = isRight ? +1 : -1;

				neighbours |= (IsVisibleBlock((ushort)(worldX + outwardX), worldY, (ushort)(worldZ + rightZ)) ? 1 : 0) << 8;
				neighbours |= (IsVisibleBlock((ushort)(worldX + outwardX), worldY, (ushort)(worldZ + leftZ)) ? 1 : 0) << 9;
				neighbours |= (IsVisibleBlock((ushort)(worldX + outwardX), (ushort)(worldY + 1), worldZ) ? 1 : 0) << 10;
				neighbours |= (IsVisibleBlock((ushort)(worldX + outwardX), (ushort)(worldY - 1), worldZ) ? 1 : 0) << 11;

				// vd: углы "вправо"
				neighbours |= (IsVisibleBlock((ushort)(worldX + outwardX), (ushort)(worldY + 1), (ushort)(worldZ + rightZ)) ? 1 : 0) << 12;
				neighbours |= (IsVisibleBlock((ushort)(worldX + outwardX), (ushort)(worldY + 1), (ushort)(worldZ + leftZ)) ? 1 : 0) << 13;
				neighbours |= (IsVisibleBlock((ushort)(worldX + outwardX), (ushort)(worldY - 1), (ushort)(worldZ + leftZ)) ? 1 : 0) << 14;
				neighbours |= (IsVisibleBlock((ushort)(worldX + outwardX), (ushort)(worldY - 1), (ushort)(worldZ + rightZ)) ? 1 : 0) << 15;
			}

			return neighbours;
		}

		private unsafe float UInt32ToFloat(uint value)
		{
			return *(float*)&value;
		}
	}
}
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace VoxelMap
{
	[BurstCompile]
	public unsafe struct FillChunkMeshJobParallel : IJobFor
	{
		private MapData _mapData;
		private readonly NativeArray<ChunkData> _chunkDataArray;

		public FillChunkMeshJobParallel(MapData mapData, NativeArray<ChunkData> chunkDataArray)
		{
			_mapData = mapData;
			_chunkDataArray = chunkDataArray;
		}

		public void Execute(int index)
		{
			int chunkIndex = _chunkDataArray[index].Index;
			int vertexCount = 0;

			var chunkOffset = new Vector3Ushort(
				(ushort)(chunkIndex / (_mapData.Depth * _mapData.Height / Chunk.ChunkSizeSquared)),
				(ushort)(chunkIndex / (_mapData.Depth / Chunk.ChunkSize) % (_mapData.Height / Chunk.ChunkSize)),
				(ushort)(chunkIndex % (_mapData.Depth / Chunk.ChunkSize))) * Chunk.ChunkSize;

			// Получаем указатели напрямую
			var verticesPtr = (VertexData*)_chunkDataArray[index].VerticesArray.ToPointer();
			var indicesPtr = (int*)_chunkDataArray[index].IndicesArray.ToPointer();

			for (var i = 0; i < Chunk.ChunkSizeCubed; i++)
			{
				int z = i % Chunk.ChunkSize;
				int y = i / Chunk.ChunkSize % Chunk.ChunkSize;
				int x = i / Chunk.ChunkSizeSquared;
				int voxelIndex = chunkIndex * Chunk.ChunkSizeCubed + i;
				Face faces = _mapData.Faces[voxelIndex];
				Color32 color = _mapData[voxelIndex].Color;

				if (faces == Face.None)
				{
					continue;
				}

				AddVoxel(x, y, z, faces, color, verticesPtr, indicesPtr, chunkOffset, ref vertexCount);
			}
		}

		private void AddVoxel(int x, int y, int z, Face faces, Color32 color,
			VertexData* verticesPtr, int* indicesPtr,
			Vector3Ushort chunkOffset, ref int vertexCount)
		{
			if (FaceExtensions.HasFlag(faces, Face.Top))
			{
				GenerateTopSide(x, y, z, color, verticesPtr, vertexCount, chunkOffset);
				vertexCount += 4;
				SwapAmbientOcclusionIfNeeded(verticesPtr, vertexCount);
				AddTriangles(indicesPtr, vertexCount);
			}

			if (FaceExtensions.HasFlag(faces, Face.Bottom))
			{
				GenerateBottomSide(x, y, z, color, verticesPtr, vertexCount, chunkOffset);
				vertexCount += 4;
				SwapAmbientOcclusionIfNeeded(verticesPtr, vertexCount);
				AddTriangles(indicesPtr, vertexCount);
			}

			if (FaceExtensions.HasFlag(faces, Face.Front))
			{
				GenerateFrontSide(x, y, z, color, verticesPtr, vertexCount, chunkOffset);
				vertexCount += 4;
				SwapAmbientOcclusionIfNeeded(verticesPtr, vertexCount);
				AddTriangles(indicesPtr, vertexCount);
			}

			if (FaceExtensions.HasFlag(faces, Face.Back))
			{
				GenerateBackSide(x, y, z, color, verticesPtr, vertexCount, chunkOffset);
				vertexCount += 4;
				SwapAmbientOcclusionIfNeeded(verticesPtr, vertexCount);
				AddTriangles(indicesPtr, vertexCount);
			}

			if (FaceExtensions.HasFlag(faces, Face.Right))
			{
				GenerateRightSide(x, y, z, color, verticesPtr, vertexCount, chunkOffset);
				vertexCount += 4;
				SwapAmbientOcclusionIfNeeded(verticesPtr, vertexCount);
				AddTriangles(indicesPtr, vertexCount);
			}

			if (FaceExtensions.HasFlag(faces, Face.Left))
			{
				GenerateLeftSide(x, y, z, color, verticesPtr, vertexCount, chunkOffset);
				vertexCount += 4;
				SwapAmbientOcclusionIfNeeded(verticesPtr, vertexCount);
				AddTriangles(indicesPtr, vertexCount);
			}
		}

		private void GenerateTopSide(int x, int y, int z, Color32 color, VertexData* verticesPtr, int vertexCount, Vector3Ushort chunkOffset)
		{
			float neighbours = UInt32ToFloat((uint)GetNeighbours(x, y, z, Face.Top, chunkOffset));

			UnsafeUtility.WriteArrayElement(verticesPtr, vertexCount + 0, new VertexData
			{
				Position = new Vector3(x, y + 1, z), Normal = Vector3.up,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, -1, 1, -1, Vector3.up, chunkOffset) << 24),
				UV = new Vector2(0, 1), Neighbours = neighbours
			});
			UnsafeUtility.WriteArrayElement(verticesPtr, vertexCount + 1, new VertexData
			{
				Position = new Vector3(x, y + 1, z + 1), Normal = Vector3.up,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, -1, 1, 1, Vector3.up, chunkOffset) << 24),
				UV = new Vector2(0, 0), Neighbours = neighbours
			});
			UnsafeUtility.WriteArrayElement(verticesPtr, vertexCount + 2, new VertexData
			{
				Position = new Vector3(x + 1, y + 1, z), Normal = Vector3.up,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, 1, 1, -1, Vector3.up, chunkOffset) << 24),
				UV = new Vector2(1, 1), Neighbours = neighbours
			});
			UnsafeUtility.WriteArrayElement(verticesPtr, vertexCount + 3, new VertexData
			{
				Position = new Vector3(x + 1, y + 1, z + 1), Normal = Vector3.up,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, 1, 1, 1, Vector3.up, chunkOffset) << 24),
				UV = new Vector2(1, 0), Neighbours = neighbours
			});
		}

		// Аналогично перепишите остальные Generate*Side методы...

		private void GenerateBottomSide(int x, int y, int z, Color32 color, VertexData* verticesPtr, int vertexCount, Vector3Ushort chunkOffset)
		{
			float neighbours = UInt32ToFloat((uint)GetNeighbours(x, y, z, Face.Bottom, chunkOffset));

			UnsafeUtility.WriteArrayElement(verticesPtr, vertexCount + 0, new VertexData
			{
				Position = new Vector3(x, y, z), Normal = Vector3.down,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, -1, -1, -1, Vector3.down, chunkOffset) << 24),
				UV = new Vector2(1, 1), Neighbours = neighbours
			});
			UnsafeUtility.WriteArrayElement(verticesPtr, vertexCount + 1, new VertexData
			{
				Position = new Vector3(x + 1, y, z), Normal = Vector3.down,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, 1, -1, -1, Vector3.down, chunkOffset) << 24),
				UV = new Vector2(0, 1), Neighbours = neighbours
			});
			UnsafeUtility.WriteArrayElement(verticesPtr, vertexCount + 2, new VertexData
			{
				Position = new Vector3(x, y, z + 1), Normal = Vector3.down,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, -1, -1, 1, Vector3.down, chunkOffset) << 24),
				UV = new Vector2(1, 0), Neighbours = neighbours
			});
			UnsafeUtility.WriteArrayElement(verticesPtr, vertexCount + 3, new VertexData
			{
				Position = new Vector3(x + 1, y, z + 1), Normal = Vector3.down,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, 1, -1, 1, Vector3.down, chunkOffset) << 24),
				UV = new Vector2(0, 0), Neighbours = neighbours
			});
		}

		private void GenerateFrontSide(int x, int y, int z, Color32 color, VertexData* verticesPtr, int vertexCount, Vector3Ushort chunkOffset)
		{
			float neighbours = UInt32ToFloat((uint)GetNeighbours(x, y, z, Face.Front, chunkOffset));

			UnsafeUtility.WriteArrayElement(verticesPtr, vertexCount + 0, new VertexData
			{
				Position = new Vector3(x, y, z + 1), Normal = Vector3.forward,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, -1, -1, 1, Vector3.forward, chunkOffset) << 24),
				UV = new Vector2(1, 0), Neighbours = neighbours
			});
			UnsafeUtility.WriteArrayElement(verticesPtr, vertexCount + 1, new VertexData
			{
				Position = new Vector3(x + 1, y, z + 1), Normal = Vector3.forward,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, 1, -1, 1, Vector3.forward, chunkOffset) << 24),
				UV = new Vector2(0, 0), Neighbours = neighbours
			});
			UnsafeUtility.WriteArrayElement(verticesPtr, vertexCount + 2, new VertexData
			{
				Position = new Vector3(x, y + 1, z + 1), Normal = Vector3.forward,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, -1, 1, 1, Vector3.forward, chunkOffset) << 24),
				UV = new Vector2(1, 1), Neighbours = neighbours
			});
			UnsafeUtility.WriteArrayElement(verticesPtr, vertexCount + 3, new VertexData
			{
				Position = new Vector3(x + 1, y + 1, z + 1), Normal = Vector3.forward,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, 1, 1, 1, Vector3.forward, chunkOffset) << 24),
				UV = new Vector2(0, 1), Neighbours = neighbours
			});
		}

		private void GenerateBackSide(int x, int y, int z, Color32 color, VertexData* verticesPtr, int vertexCount, Vector3Ushort chunkOffset)
		{
			float neighbours = UInt32ToFloat((uint)GetNeighbours(x, y, z, Face.Back, chunkOffset));

			UnsafeUtility.WriteArrayElement(verticesPtr, vertexCount + 0, new VertexData
			{
				Position = new Vector3(x, y, z), Normal = Vector3.back,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, -1, -1, -1, Vector3.back, chunkOffset) << 24),
				UV = new Vector2(0, 0), Neighbours = neighbours
			});
			UnsafeUtility.WriteArrayElement(verticesPtr, vertexCount + 1, new VertexData
			{
				Position = new Vector3(x, y + 1, z), Normal = Vector3.back,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, -1, 1, -1, Vector3.back, chunkOffset) << 24),
				UV = new Vector2(0, 1), Neighbours = neighbours
			});
			UnsafeUtility.WriteArrayElement(verticesPtr, vertexCount + 2, new VertexData
			{
				Position = new Vector3(x + 1, y, z), Normal = Vector3.back,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, 1, -1, -1, Vector3.back, chunkOffset) << 24),
				UV = new Vector2(1, 0), Neighbours = neighbours
			});
			UnsafeUtility.WriteArrayElement(verticesPtr, vertexCount + 3, new VertexData
			{
				Position = new Vector3(x + 1, y + 1, z), Normal = Vector3.back,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, 1, 1, -1, Vector3.back, chunkOffset) << 24),
				UV = new Vector2(1, 1), Neighbours = neighbours
			});
		}

		private void GenerateRightSide(int x, int y, int z, Color32 color, VertexData* verticesPtr, int vertexCount, Vector3Ushort chunkOffset)
		{
			float neighbours = UInt32ToFloat((uint)GetNeighbours(x, y, z, Face.Right, chunkOffset));

			UnsafeUtility.WriteArrayElement(verticesPtr, vertexCount + 0, new VertexData
			{
				Position = new Vector3(x + 1, y, z), Normal = Vector3.right,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, 1, -1, -1, Vector3.right, chunkOffset) << 24),
				UV = new Vector2(0, 0), Neighbours = neighbours
			});
			UnsafeUtility.WriteArrayElement(verticesPtr, vertexCount + 1, new VertexData
			{
				Position = new Vector3(x + 1, y + 1, z), Normal = Vector3.right,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, 1, 1, -1, Vector3.right, chunkOffset) << 24),
				UV = new Vector2(0, 1), Neighbours = neighbours
			});
			UnsafeUtility.WriteArrayElement(verticesPtr, vertexCount + 2, new VertexData
			{
				Position = new Vector3(x + 1, y, z + 1), Normal = Vector3.right,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, 1, -1, 1, Vector3.right, chunkOffset) << 24),
				UV = new Vector2(1, 0), Neighbours = neighbours
			});
			UnsafeUtility.WriteArrayElement(verticesPtr, vertexCount + 3, new VertexData
			{
				Position = new Vector3(x + 1, y + 1, z + 1), Normal = Vector3.right,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, 1, 1, 1, Vector3.right, chunkOffset) << 24),
				UV = new Vector2(1, 1), Neighbours = neighbours
			});
		}

		private void GenerateLeftSide(int x, int y, int z, Color32 color, VertexData* verticesPtr, int vertexCount, Vector3Ushort chunkOffset)
		{
			float neighbours = UInt32ToFloat((uint)GetNeighbours(x, y, z, Face.Left, chunkOffset));

			UnsafeUtility.WriteArrayElement(verticesPtr, vertexCount + 0, new VertexData
			{
				Position = new Vector3(x, y, z), Normal = Vector3.left,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, -1, -1, -1, Vector3.left, chunkOffset) << 24),
				UV = new Vector2(1, 0), Neighbours = neighbours
			});
			UnsafeUtility.WriteArrayElement(verticesPtr, vertexCount + 1, new VertexData
			{
				Position = new Vector3(x, y, z + 1), Normal = Vector3.left,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, -1, -1, 1, Vector3.left, chunkOffset) << 24),
				UV = new Vector2(0, 0), Neighbours = neighbours
			});
			UnsafeUtility.WriteArrayElement(verticesPtr, vertexCount + 2, new VertexData
			{
				Position = new Vector3(x, y + 1, z), Normal = Vector3.left,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, -1, 1, -1, Vector3.left, chunkOffset) << 24),
				UV = new Vector2(1, 1), Neighbours = neighbours
			});
			UnsafeUtility.WriteArrayElement(verticesPtr, vertexCount + 3, new VertexData
			{
				Position = new Vector3(x, y + 1, z + 1), Normal = Vector3.left,
				Color = (uint)(color.r | color.g << 8 | color.b << 16 | GetVertexAO(x, y, z, -1, 1, 1, Vector3.left, chunkOffset) << 24),
				UV = new Vector2(0, 1), Neighbours = neighbours
			});
		}

		private void SwapAmbientOcclusionIfNeeded(VertexData* verticesPtr, int vertexCount)
		{
			var v0 = UnsafeUtility.ReadArrayElement<VertexData>(verticesPtr, vertexCount - 4);
			var v1 = UnsafeUtility.ReadArrayElement<VertexData>(verticesPtr, vertexCount - 3);
			var v2 = UnsafeUtility.ReadArrayElement<VertexData>(verticesPtr, vertexCount - 2);
			var v3 = UnsafeUtility.ReadArrayElement<VertexData>(verticesPtr, vertexCount - 1);

			if (v0.AO + v3.AO > v1.AO + v2.AO)
			{
				UnsafeUtility.WriteArrayElement(verticesPtr, vertexCount - 4, v2);
				UnsafeUtility.WriteArrayElement(verticesPtr, vertexCount - 3, v0);
				UnsafeUtility.WriteArrayElement(verticesPtr, vertexCount - 2, v3);
				UnsafeUtility.WriteArrayElement(verticesPtr, vertexCount - 1, v1);
			}
		}

		private void AddTriangles(int* indicesPtr, int vertexCount)
		{
			int baseIndex = 6 * (vertexCount - 4) / 4;
			UnsafeUtility.WriteArrayElement(indicesPtr, baseIndex + 0, vertexCount - 4);
			UnsafeUtility.WriteArrayElement(indicesPtr, baseIndex + 1, vertexCount - 3);
			UnsafeUtility.WriteArrayElement(indicesPtr, baseIndex + 2, vertexCount - 2);
			UnsafeUtility.WriteArrayElement(indicesPtr, baseIndex + 3, vertexCount - 3);
			UnsafeUtility.WriteArrayElement(indicesPtr, baseIndex + 4, vertexCount - 1);
			UnsafeUtility.WriteArrayElement(indicesPtr, baseIndex + 5, vertexCount - 2);
		}

		// ... остальные методы (IsVisibleBlock, GetVertexAO, GetNeighbours, UInt32ToFloat) без изменений ...

		private bool IsVisibleBlock(ushort x, ushort y, ushort z)
		{
			return _mapData.IsValidPosition(x, y, z) && _mapData[x, y, z].IsSolid();
		}

		private byte GetVertexAO(int x, int y, int z, int xOffset, int yOffset, int zOffset, Vector3 normal, Vector3Ushort chunkOffset)
		{
			var worldX = (ushort)(x + chunkOffset.x);
			var worldY = (ushort)(y + chunkOffset.y);
			var worldZ = (ushort)(z + chunkOffset.z);

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

		private int GetNeighbours(int x, int y, int z, Face face, Vector3Ushort chunkOffset)
		{
			var worldX = (ushort)(x + chunkOffset.x);
			var worldY = (ushort)(y + chunkOffset.y);
			var worldZ = (ushort)(z + chunkOffset.z);
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

		private float UInt32ToFloat(uint value)
		{
			return *(float*)&value;
		}
	}
}
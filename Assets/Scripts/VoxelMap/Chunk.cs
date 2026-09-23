using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
namespace VoxelMap
{
	public class Chunk
	{
		public const int ChunkSize = 32;
		public const int ChunkSizeSquared = 1024;
		public const int ChunkSizeCubed = 32768;

		public Mesh Mesh { get; }
		public Vector3Int Position => Vector3Int.FloorToInt(_meshFilter.gameObject.transform.position);
		public int FaceCount { get; set; }

		private readonly int _index;
		private readonly MeshCollider _collider;
		private readonly MapData _mapData;
		private readonly MeshFilter _meshFilter;
		private int VertexCount => 4 * FaceCount;
		private int IndexCount => 6 * FaceCount;
		private Bounds Bounds => new Bounds((float)ChunkSize / 2 * Vector3.one, ChunkSize * Vector3.one);

		public Chunk(int index, MapData mapData, MeshFilter meshFilter, MeshCollider collider, int faceCount)
		{
			_index = index;
			_mapData = mapData;
			_meshFilter = meshFilter;
			_collider = collider;
			FaceCount = faceCount;
			Mesh = new Mesh();
			Mesh.bounds = Bounds;
		}

		public void Regenerate()
		{
			Mesh.MeshDataArray meshArray = Mesh.AllocateWritableMeshData(1);
			Mesh.MeshData meshData = meshArray[0];
			SetupMeshData(meshData);
			var vertices = meshData.GetVertexData<VertexData>(); 
			var indexes = meshData.GetIndexData<int>();

			var fillJob = new FillChunkMeshJob(_mapData, _index, vertices, indexes);
			fillJob.Schedule().Complete();
			
			ApplyMeshData(meshArray, meshData);

			var bakeJob = new BakeChunkColliderJob(Mesh.GetInstanceID());
			bakeJob.Schedule().Complete();
			ApplyMeshCollider();
		}

		public async UniTask RegenerateAsync(CancellationToken cancellationToken = default)
		{
			Mesh.MeshDataArray meshArray = Mesh.AllocateWritableMeshData(1);
			Mesh.MeshData meshData = meshArray[0];

			SetupMeshData(meshData);

			var vertices = meshData.GetVertexData<VertexData>();
			var indexes = meshData.GetIndexData<int>();

			var fillJob = new FillChunkMeshJob(_mapData, _index, vertices, indexes);
			await fillJob.Schedule().ToUniTask(PlayerLoopTiming.Update);
			
			cancellationToken.ThrowIfCancellationRequested();

			ApplyMeshData(meshArray, meshData);

			var bakeJob = new BakeChunkColliderJob(Mesh.GetInstanceID());
			await bakeJob.Schedule().ToUniTask(PlayerLoopTiming.Update);
			
			cancellationToken.ThrowIfCancellationRequested();

			ApplyMeshCollider();
		}

		public static unsafe void RegenerateParallel(MapData mapData, ICollection<Chunk> chunks)
		{
			var chunkDataArray = new NativeArray<ChunkData>(chunks.Count, Allocator.TempJob);
			var meshIndexes = new NativeArray<int>(chunks.Count, Allocator.TempJob);
			var meshContexts = ArrayPool<(Mesh.MeshDataArray meshDataArray, Mesh.MeshData meshData)>.Shared.Rent(chunks.Count);
			
			try
			{
				var chunkNumber = 0;

				foreach (Chunk chunk in chunks)
				{
					Mesh.MeshDataArray meshArray = Mesh.AllocateWritableMeshData(1);
					Mesh.MeshData meshData = meshArray[0];
					
					meshContexts[chunkNumber] = (meshArray, meshData);

					chunk.SetupMeshData(meshData);

					var vertices = meshData.GetVertexData<VertexData>();
					var indexes = meshData.GetIndexData<int>();

					var verticesPointer = new IntPtr(vertices.GetUnsafePtr());
					var indexesPointer = new IntPtr(indexes.GetUnsafePtr());
					int chunkIndex = chunk._index;

					chunkDataArray[chunkNumber] = new ChunkData(verticesPointer, vertices.Length, 
						indexesPointer, indexes.Length, chunkIndex);
					chunkNumber++;
				}

				var fillJob = new FillChunkMeshJobParallel(mapData, chunkDataArray);
				fillJob.ScheduleParallel(chunkDataArray.Length, 2, default).Complete();

				chunkNumber = 0;
				
				foreach (Chunk chunk in chunks)
				{
					chunk.ApplyMeshData(meshContexts[chunkNumber].meshDataArray, meshContexts[chunkNumber].meshData);
					meshIndexes[chunkNumber] = chunk.Mesh.GetInstanceID();
					chunkNumber++;
				}

				var bakeJob = new BakeChunkColliderParallelJob(meshIndexes);
				bakeJob.ScheduleParallel(meshIndexes.Length, 2, default).Complete();

				foreach (Chunk chunk in chunks)
				{
					chunk.ApplyMeshCollider();
				}
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
			finally
			{
				chunkDataArray.Dispose();
				meshIndexes.Dispose();
				ArrayPool<(Mesh.MeshDataArray meshDataArray, Mesh.MeshData meshData)>.Shared.Return(meshContexts);
			}
		}

		private void SetupMeshData(Mesh.MeshData meshData)
		{
			var attributes = new NativeArray<VertexAttributeDescriptor>(5, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			attributes[0] = new VertexAttributeDescriptor(VertexAttribute.Position, dimension:3);
			attributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, dimension:3);
			attributes[2] = new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, dimension: 4);
			attributes[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, dimension: 2);
			attributes[4] = new VertexAttributeDescriptor(VertexAttribute.TexCoord1, dimension: 1);

			meshData.SetVertexBufferParams(VertexCount, attributes);
			meshData.SetIndexBufferParams(IndexCount, IndexFormat.UInt32);

			attributes.Dispose();
		}

		private void ApplyMeshData(Mesh.MeshDataArray meshArray, Mesh.MeshData meshData)
		{
			meshData.subMeshCount = 1;
			meshData.SetSubMesh(0, new SubMeshDescriptor(0, IndexCount)
			{
				bounds = Bounds,
				vertexCount = VertexCount,
			}, MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontNotifyMeshUsers);

			Mesh.ApplyAndDisposeWritableMeshData(meshArray, Mesh);
		}

		private void ApplyMeshCollider()
		{
			_collider.sharedMesh = null;

			if (VertexCount > 0)
			{
				_meshFilter.mesh = Mesh;
				_collider.sharedMesh = Mesh;
			}
		}
	}
}
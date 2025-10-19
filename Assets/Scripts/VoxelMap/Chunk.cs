using Cysharp.Threading.Tasks;
using Unity.Collections;
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

		private readonly int _chunkIndex;
		private readonly MeshCollider _collider;
		private readonly MapData _mapData;
		private readonly MeshFilter _meshFilter;
		private int VertexCount => 4 * FaceCount;
		private int IndexCount => 6 * FaceCount;
		private Bounds Bounds => new Bounds((float)ChunkSize / 2 * Vector3.one, ChunkSize * Vector3.one);

		public Chunk(int chunkIndex, MapData mapData, MeshFilter meshFilter, MeshCollider collider, int faceCount)
		{
			_chunkIndex = chunkIndex;
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

			var regenerateJob = new RegenerateChunkJob(_mapData, _chunkIndex, vertices, indexes);
			regenerateJob.Schedule().Complete();
			
			ApplyMeshData(meshData, meshArray);

			var bakeJob = new BakeChunkColliderJob(Mesh.GetInstanceID());
			bakeJob.Schedule().Complete();

			ApplyMeshCollider();
		}

		public async UniTask RegenerateAsync()
		{
			Mesh.MeshDataArray meshArray = Mesh.AllocateWritableMeshData(1);
			Mesh.MeshData meshData = meshArray[0];

			SetupMeshData(meshData);

			var vertices = meshData.GetVertexData<VertexData>();
			var indexes = meshData.GetIndexData<int>();

			var regenerateChunkJob = new RegenerateChunkJob(_mapData, _chunkIndex, vertices, indexes);
			await regenerateChunkJob.Schedule().ToUniTask(PlayerLoopTiming.Update);

			ApplyMeshData(meshData, meshArray);

			var bakeJob = new BakeChunkColliderJob(Mesh.GetInstanceID());
			await bakeJob.Schedule().ToUniTask(PlayerLoopTiming.Update);

			ApplyMeshCollider();
		}

		private void SetupMeshData(Mesh.MeshData meshData)
		{
			var attributes = new NativeArray<VertexAttributeDescriptor>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			attributes[0] = new VertexAttributeDescriptor(VertexAttribute.Position, dimension:3);
			attributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, dimension:3);
			attributes[2] = new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, dimension: 4);
			attributes[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, dimension: 2);

			meshData.SetVertexBufferParams(VertexCount, attributes);
			meshData.SetIndexBufferParams(IndexCount, IndexFormat.UInt32);

			attributes.Dispose();
		}

		private void ApplyMeshData(Mesh.MeshData meshData, Mesh.MeshDataArray meshArray)
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
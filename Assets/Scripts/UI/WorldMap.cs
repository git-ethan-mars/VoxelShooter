using System.Text;
using GamePlay;
using R3;
using Reflex.Attributes;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;
using VoxelMap;
namespace UI
{
	public class WorldMap : MonoBehaviour
	{
		private const string ClearHeight = "ClearHeight";
		private const string UpdateHeight = "UpdateHeight";
		private const string UpdateChunk = "UpdateChunk";
		
		private static readonly int HeightBuffer = Shader.PropertyToID("HeightBuffer");
		private static readonly int VertexBuffer = Shader.PropertyToID("VertexBuffer");
		private static readonly int MapTexture = Shader.PropertyToID("MapTexture");
		private static readonly int MeshPositionOffset = Shader.PropertyToID("MeshPositionOffset");
		private static readonly int ChunkSize = Shader.PropertyToID("ChunkSize");

		[SerializeField] private ComputeShader computeShader;
		[SerializeField] private RawImage mapImage;

		private MapProvider _mapProvider;
		private EntityContainerService _entityContainer;

		private ComputeBuffer _heightBuffer;
		private int _updateHeightKernel;
		private int _updateChunkKernel;
		private int _clearHeightKernel;

		[field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }
		public RenderTexture MainTexture { get; private set; }


		[Inject]
		private void Construct(MapProvider mapProvider)
		{
			_mapProvider = mapProvider;

			MainTexture = new RenderTexture(_mapProvider.Map.Width, _mapProvider.Map.Depth, 0, RenderTextureFormat.ARGB32)
			{
				enableRandomWrite = true,
				filterMode = FilterMode.Point
			};

			_heightBuffer = new ComputeBuffer(Chunk.ChunkSizeSquared, sizeof(uint));
		}

		private void Start()
		{
			MainTexture.Create();
			mapImage.texture = MainTexture;

			for (int x = 0; x < _mapProvider.Map.Width / Chunk.ChunkSize; x++)
			{
				for (var z = 0; z < _mapProvider.Map.Depth / Chunk.ChunkSize; z++)
				{ 
					//RefreshColumn(x, z);
				}
			}

			_mapProvider.Map.ChunkUpdated.Subscribe(OnChunkUpdated).AddTo(this);


			_clearHeightKernel = computeShader.FindKernel(ClearHeight);
			_updateHeightKernel = computeShader.FindKernel(UpdateHeight);
			_updateChunkKernel = computeShader.FindKernel(UpdateChunk);

			computeShader.SetFloat(ChunkSize, Chunk.ChunkSize);
		}

		private void OnChunkUpdated(Chunk chunk)
		{
			RefreshColumn(chunk.Position.x / Chunk.ChunkSize, chunk.Position.z / Chunk.ChunkSize);
		}

		private void RefreshColumn(int x, int z)
		{
			computeShader.SetBuffer(_clearHeightKernel, HeightBuffer, _heightBuffer);
			
			//RenderDoc.BeginCaptureRenderDoc(SceneView.lastActiveSceneView);
			computeShader.Dispatch(_clearHeightKernel, Chunk.ChunkSize / 8, Chunk.ChunkSize / 8, 1);
			//RenderDoc.EndCaptureRenderDoc(SceneView.lastActiveSceneView);
			int maxYChunkIndex = _mapProvider.Map.Height / Chunk.ChunkSize;
			int maxZChunkIndex = _mapProvider.Map.Depth / Chunk.ChunkSize;
			int chunkIndex = x * maxYChunkIndex * maxZChunkIndex + z;

			int blockStartIndex = chunkIndex / (maxYChunkIndex * maxZChunkIndex) * maxYChunkIndex * maxZChunkIndex;
			int chunkIndexOffset = (chunkIndex - blockStartIndex) % maxZChunkIndex;

			for (var i = blockStartIndex + chunkIndexOffset;
			     i < blockStartIndex + chunkIndexOffset + maxYChunkIndex * maxZChunkIndex;
			     i += maxZChunkIndex)
			{
				Chunk chunk = _mapProvider.Map.Chunks[i];

				if (chunk.Mesh.vertexCount == 0)
				{
					continue;
				}

				chunk.Mesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;
				using GraphicsBuffer vertexBuffer = chunk.Mesh.GetVertexBuffer(0);
				computeShader.SetBuffer(_updateHeightKernel, VertexBuffer, vertexBuffer);
				computeShader.SetBuffer(_updateHeightKernel, HeightBuffer, _heightBuffer);
				computeShader.SetFloats(MeshPositionOffset, chunk.Position.x, chunk.Position.y, chunk.Position.z);
				computeShader.Dispatch(_updateHeightKernel, // 0-3 = 0, 4-7 = 1; 8-11 = 2;
					Mathf.CeilToInt((float)vertexBuffer.count / 4 / 16), 1, 1);
			}

			/*for (var i = blockStartIndex + chunkIndexOffset;
			     i < blockStartIndex + chunkIndexOffset + maxYChunkIndex * maxZChunkIndex;
			     i += maxZChunkIndex)
			{
				Chunk chunk = _mapProvider.Map.Chunks[i];

				if (chunk.Mesh.vertexCount == 0)
				{
					continue;
				}

				using GraphicsBuffer vertexBuffer = chunk.Mesh.GetVertexBuffer(0);
				computeShader.SetBuffer(updateChunk, VertexBuffer, vertexBuffer);
				computeShader.SetBuffer(updateChunk, HeightBuffer, _heightBuffer);
				computeShader.SetTexture(updateChunk, MapTexture, MainTexture);
				computeShader.SetFloats(MeshPositionOffset, chunk.Position.x, chunk.Position.y, chunk.Position.z);
				//RenderDoc.BeginCaptureRenderDoc(SceneView.lastActiveSceneView);
				computeShader.Dispatch(updateChunk,
					Mathf.CeilToInt((float)vertexBuffer.count / 16 / 4), 1, 1);
				//RenderDoc.EndCaptureRenderDoc(SceneView.lastActiveSceneView);
				Debug.Log($"{x} {z}");

			}*/
		}

		private void OnDestroy()
		{
			_heightBuffer.Release();
			MainTexture.Release();
		}
	}
}
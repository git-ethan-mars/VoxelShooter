using GamePlay;
using R3;
using Reflex.Attributes;
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
		private static readonly int ChunkPosition = Shader.PropertyToID("ChunkPosition");
		private static readonly int ChunkSize = Shader.PropertyToID("ChunkSize");
		private static readonly int BufferSize = Shader.PropertyToID("BufferSize");
		private static readonly int GridColor = Shader.PropertyToID("GridColor");

		[SerializeField] private ComputeShader computeShader;
		[SerializeField] private RawImage mapImage;
		[SerializeField] private Color gridColor;

		private MapProvider _mapProvider;
		private EntityContainer _entityContainer;

		private ComputeBuffer _heightBuffer;
		private int _clearHeightKernel;
		private int _updateHeightKernel;
		private int _updateChunkKernel;
		private int _drawGridKernel;

		[field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }
		public RenderTexture MainTexture { get; private set; }

		[Inject]
		private void Construct(MapProvider mapProvider)
		{
			_mapProvider = mapProvider;

			_heightBuffer = new ComputeBuffer(Chunk.ChunkSizeSquared, sizeof(float));
			_clearHeightKernel = computeShader.FindKernel(ClearHeight);
			_updateHeightKernel = computeShader.FindKernel(UpdateHeight);
			_updateChunkKernel = computeShader.FindKernel(UpdateChunk);

			computeShader.SetInt(ChunkSize, Chunk.ChunkSize);
			computeShader.SetVector(GridColor, gridColor);
		}

		internal void OnMapChanged(Map map)
		{
			if (MainTexture != null)
			{
				MainTexture.Release();
			}

			MainTexture = new RenderTexture(map.Width, map.Depth, 0, RenderTextureFormat.ARGB32)
			{
				enableRandomWrite = true,
				filterMode = FilterMode.Point
			};

			MainTexture.Create();
			mapImage.texture = MainTexture;

			map.ChunkUpdated.Subscribe(OnChunkUpdated).AddTo(map);

			for (int x = 0; x < map.Width / Chunk.ChunkSize; x++)
			{
				for (var z = 0; z < map.Depth / Chunk.ChunkSize; z++)
				{
					RefreshChunkColumn(x, z);
				}
			}
		}

		private void RefreshChunkColumn(int x, int z)
		{
			computeShader.SetBuffer(_clearHeightKernel, HeightBuffer, _heightBuffer);
			computeShader.Dispatch(_clearHeightKernel, Chunk.ChunkSize / 8, Chunk.ChunkSize / 8, 1);

			for (var y = 0; y < _mapProvider.Map.CurrentValue.Height / Chunk.ChunkSize; y++)
			{
				int chunkIndex = GetChunkIndex(x, y, z);
				ProcessChunk(chunkIndex);
			}
		}

		private void ProcessChunk(int chunkIndex)
		{
			Chunk chunk = _mapProvider.Map.CurrentValue.Chunks[chunkIndex];

			if (chunk.Mesh.vertexCount == 0)
			{
				return;
			}

			chunk.Mesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;
			using var vertexBuffer = chunk.Mesh.GetVertexBuffer(0);

			computeShader.SetBuffer(_updateHeightKernel, VertexBuffer, vertexBuffer);
			computeShader.SetBuffer(_updateHeightKernel, HeightBuffer, _heightBuffer);
			computeShader.SetFloats(ChunkPosition, chunk.Position.x, chunk.Position.y, chunk.Position.z);
			computeShader.SetInt(BufferSize, vertexBuffer.count);
			computeShader.Dispatch(_updateHeightKernel, Mathf.CeilToInt((float)vertexBuffer.count / 4 / 16), 1, 1);

			computeShader.SetBuffer(_updateChunkKernel, VertexBuffer, vertexBuffer);
			computeShader.SetBuffer(_updateChunkKernel, HeightBuffer, _heightBuffer);
			computeShader.SetFloats(ChunkPosition, chunk.Position.x, chunk.Position.y, chunk.Position.z);
			computeShader.SetInt(BufferSize, vertexBuffer.count);
			computeShader.SetTexture(_updateChunkKernel, MapTexture, MainTexture);
			computeShader.Dispatch(_updateChunkKernel, Mathf.CeilToInt((float)vertexBuffer.count / 4 / 16), 1, 1);
		}

		private void OnChunkUpdated(Chunk chunk)
		{
			RefreshChunkColumn(chunk.Position.x / Chunk.ChunkSize, chunk.Position.z / Chunk.ChunkSize);
		}

		private int GetChunkIndex(int x, int y, int z)
		{
			return x * (_mapProvider.Map.CurrentValue.Height * _mapProvider.Map.CurrentValue.Depth / Chunk.ChunkSizeSquared)
			       + y * (_mapProvider.Map.CurrentValue.Depth / Chunk.ChunkSize) + z;
		}

		private void OnDestroy()
		{
			_heightBuffer.Release();
			MainTexture?.Release();
		}
	}
}

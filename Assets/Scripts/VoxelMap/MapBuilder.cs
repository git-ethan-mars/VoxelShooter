using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
namespace VoxelMap
{
	public class MapBuilder
	{
		private const string ChunksContainerName = "Chunks";

		private readonly IMapFactory _mapFactory;
		private readonly MapData _mapData;

		private bool _waterColorChanging;
		private Color32 _waterColor = VoxelData.Air.Color;

		private bool _innerColorChanging;
		private Color32 _innerColor;

		private bool _enableWalls;
		private List<SpawnPointData> _spawnPoints;

		private AmbientData _ambient;
		private FogData _fog;
		private LightData _light;
		private Material _skybox;

		public MapBuilder(IMapFactory mapFactory, MapData mapData)
		{
			_mapData = mapData;
			_mapFactory = mapFactory;
		}

		public MapBuilder WithWaterColor(Color32 color)
		{
			_waterColorChanging = true;
			_waterColor = color;
			return this;
		}

		public MapBuilder WithInnerColor(Color32 color)
		{
			_innerColorChanging = true;
			_innerColor = color;
			return this;
		}

		public MapBuilder WithDirectionalLight(LightData lightData)
		{
			_light = lightData;
			return this;
		}

		public MapBuilder WithFog(FogData fogData)
		{
			_fog = fogData;
			return this;
		}

		public MapBuilder WithSkybox(Material skybox)
		{
			_skybox = skybox;
			return this;
		}

		public MapBuilder WithAmbient(AmbientData ambient)
		{
			_ambient = ambient;
			return this;
		}

		public MapBuilder WithWalls()
		{
			_enableWalls = true;
			return this;
		}

		public MapBuilder FromConfigure(MapConfigure mapConfigure)
		{
			return WithWaterColor(mapConfigure.WaterColor)
				.WithInnerColor(mapConfigure.InnerColor)
				.WithAmbient(mapConfigure.AmbientData)
				.WithDirectionalLight(mapConfigure.LightData)
				.WithFog(mapConfigure.FogData)
				.WithSkybox(mapConfigure.SkyboxMaterial)
				.WithWalls();
		}

		public MapBuilder WithSpawnPoints(List<SpawnPointData> spawnPoints)
		{
			_spawnPoints = spawnPoints;
			return this;
		}

		public Map Build(Transform container = null)
		{
			Map map = _mapFactory.CreateEmptyMap();
			map.transform.SetParent(container);

			if (_innerColorChanging)
			{
				var job = new InnerColorChangeJob(_mapData, _innerColor);
				job.ScheduleParallel(_mapData.ChunkCount, 32, default).Complete();
			}

			if (_waterColorChanging)
			{
				var waterColorJob = new WaterColorChangeJob(_mapData, _waterColor);
				waterColorJob.ScheduleParallel(_mapData.Width * _mapData.Depth, 32, default).Complete();
				var mapCenter = new Vector3((float)_mapData.Width / 2, 1, (float)_mapData.Depth / 2);
				_mapFactory.CreateWaterPlane(mapCenter, _waterColor, map.transform);
			}

			using var faceCountPerChunk = new NativeArray<int>(_mapData.ChunkCount, Allocator.TempJob);
			var calculateFacesJob = new CalculateFacesJob(_mapData, faceCountPerChunk);
			calculateFacesJob.Schedule(_mapData.ChunkCount, default).Complete();

			Chunk[] chunks = GenerateChunks(_mapData, map.transform, faceCountPerChunk);

			SetupEnvironment(_mapData, map);

			map.Construct(_mapData, chunks);
			return map;
		}

		public async UniTask<Map> BuildAsync(IProgress<float> progress = null, CancellationToken token = default)
		{
			Map map = _mapFactory.CreateEmptyMap();

			if (_innerColorChanging)
			{
				var job = new InnerColorChangeJob(_mapData, _innerColor);
				await job.ScheduleParallel(_mapData.ChunkCount, 32, default)
					.ToUniTask(PlayerLoopTiming.Update);
				token.ThrowIfCancellationRequested();
			}

			if (_waterColorChanging)
			{
				var waterColorJob = new WaterColorChangeJob(_mapData, _waterColor);
				await waterColorJob.ScheduleParallel(_mapData.Width * _mapData.Depth, 32, default)
					.ToUniTask(PlayerLoopTiming.Update);
				token.ThrowIfCancellationRequested();
				var mapCenter = new Vector3((float)_mapData.Width / 2, 1, (float)_mapData.Depth / 2);
				_mapFactory.CreateWaterPlane(mapCenter, _waterColor, map.transform);
			}

			using var faceCountPerChunk = new NativeArray<int>(_mapData.ChunkCount, Allocator.Persistent);
			var calculateFacesJob = new CalculateFacesJob(_mapData, faceCountPerChunk);
			await calculateFacesJob.Schedule(_mapData.ChunkCount, default)
				.ToUniTask(PlayerLoopTiming.Update);
			
			token.ThrowIfCancellationRequested();
			
			Chunk[] chunks = await GenerateChunksAsync(_mapData, map.transform, faceCountPerChunk, progress, token);
			
			SetupEnvironment(_mapData, map);

			map.Construct(_mapData, chunks);
			return map;
		}

		private Chunk[] GenerateChunks(MapData mapData, Transform container, NativeArray<int> faceCountPerChunk)
		{
			var chunksContainer = new GameObject(ChunksContainerName).transform;
			chunksContainer.SetParent(container);
			var chunks = new Chunk[mapData.ChunkCount];
			for (var i = 0; i < mapData.ChunkCount; i++)
			{
				Vector3 chunkPosition = ChunkIndexToPosition(i, mapData);
				GameObject chunkView = _mapFactory.CreateChunkView(chunkPosition, chunksContainer);
				chunkView.name = $"Chunk {i}";
				var meshFilter = chunkView.GetComponent<MeshFilter>();
				var meshCollider = chunkView.GetComponent<MeshCollider>();
				chunks[i] = new Chunk(i, mapData, meshFilter, meshCollider, faceCountPerChunk[i]);
				chunks[i].Regenerate();
			}

			return chunks;
		}

		private void SetupEnvironment(MapData mapData, Map map)
		{
			if (_enableWalls)
			{
				_mapFactory.CreateWalls(mapData, map.transform);
			}

			if (_spawnPoints != null)
			{
				_mapFactory.CreateSpawnPoints(_spawnPoints, map.transform);
			}

			if (_skybox != null)
			{
				Environment.ApplySkybox(_skybox);
			}

			if (!_light.Equals(default))
			{
				_mapFactory.CreateDirectionalLight(_light, map.transform);
			}

			if (!_fog.Equals(default))
			{
				Environment.ApplyFog(_fog);
			}

			if (!_ambient.Equals(default))
			{
				Environment.ApplyAmbientLighting(_ambient);
			}
		}

		private async UniTask<Chunk[]> GenerateChunksAsync(MapData mapData, Transform container, NativeArray<int> faceCountPerChunk,
			IProgress<float> progress = null, CancellationToken token = default)
		{
			var chunks = new Chunk[mapData.ChunkCount];
			var tasks = new UniTask[mapData.ChunkCount];
			var completedChunks = 0;
			var chunksContainer = new GameObject(ChunksContainerName).transform;
			chunksContainer.SetParent(container);

			for (var i = 0; i < mapData.ChunkCount; i++)
			{
				Vector3 chunkPosition = ChunkIndexToPosition(i, mapData);
				GameObject chunkView = _mapFactory.CreateChunkView(chunkPosition, chunksContainer);
				chunkView.name = $"Chunk {i}";
				var meshFilter = chunkView.GetComponent<MeshFilter>();
				var meshCollider = chunkView.GetComponent<MeshCollider>();
				chunks[i] = new Chunk(i, mapData, meshFilter, meshCollider, faceCountPerChunk[i]);
				tasks[i] = chunks[i].RegenerateAsync(token).ContinueWith(() =>
				{
					completedChunks++;
					progress?.Report(completedChunks / (float)mapData.ChunkCount);
				});
			}

			await UniTask.WhenAll(tasks);

			return chunks;
		}

		private Vector3 ChunkIndexToPosition(int index, MapData mapData)
		{
			int z = index % (mapData.Depth / Chunk.ChunkSize) * Chunk.ChunkSize;
			int y = index / (mapData.Depth / Chunk.ChunkSize) % (mapData.Height / Chunk.ChunkSize) * Chunk.ChunkSize;
			int x = index / (mapData.Depth * mapData.Height / Chunk.ChunkSizeSquared) * Chunk.ChunkSize;
			return new Vector3(x, y, z);
		}
	}
}
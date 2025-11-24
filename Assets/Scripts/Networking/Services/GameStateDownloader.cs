using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using Networking.Core;
using Networking.Messages;
using R3;
using Services;
using UnityEngine;
using VoxelMap;
using VoxelMap.Data;
namespace Networking
{
	public class GameStateDownloader
	{
		private readonly VSNetworkManager _networkManager;
		private readonly MapProvider _mapProvider;
		private readonly IMapConfigureLoader _mapConfigureLoader;
		private readonly IMapFactory _mapFactory;

		private readonly List<Voxel> _addingVoxels = new List<Voxel>();
		private readonly List<Vector3Ushort> _removingPositions = new List<Vector3Ushort>();

		public GameStateDownloader(VSNetworkManager networkManager, MapProvider mapProvider,
			IMapConfigureLoader mapConfigureLoader, IMapFactory mapFactory)
		{
			_networkManager = networkManager;
			_mapProvider = mapProvider;
			_mapConfigureLoader = mapConfigureLoader;
			_mapFactory = mapFactory;
		}

		public async UniTask<Map> DownloadMapAsync(string mapName, IProgress<float> progress = null, CancellationToken cancellationToken = default)
		{
			var tsc = new UniTaskCompletionSource<Map>();
			var request = new MapDownloadRequest();
			_networkManager.SendRequest(request);

			byte[] buffer = null;

			using IDisposable disposable = _networkManager.MessageReceived
				.OfMessageType<MapDownloadResponse>()
				.Subscribe(response => ProcessMessage(response.Message).Forget());

			async UniTask ProcessMessage(MapDownloadResponse message)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					tsc.TrySetCanceled();
				}

				buffer ??= new byte[message.TotalBytes];

				Array.Copy(message.ByteChunk, 0, buffer, message.Offset, message.ByteChunk.Length);
				progress?.Report(Mathf.Clamp01((float)(message.Offset + message.ByteChunk.Length) / message.TotalBytes));
				if (message.Offset + message.ByteChunk.Length >= message.TotalBytes)
				{
					using var memoryStream = new MemoryStream(buffer);
					MapData mapData = await MapDataReader.ReadFromStreamAsync(memoryStream, cancellationToken);
					Map map = await CreateMap(mapName, mapData, progress);
					tsc.TrySetResult(map);
				}
			}

			return await tsc.Task.AttachExternalCancellation(cancellationToken);
		}

		public async UniTask<string> DownloadMapNameAsync(CancellationToken cancellationToken = default)
		{
			var request = new MapNameRequest();
			_networkManager.SendRequest(request);
			var response = await _networkManager.MessageReceived.FirstAsync<MapNameResponse>(cancellationToken);
			return response.Message.MapName;
		}

		public async UniTask<GameSettings> DownloadGameSettings(CancellationToken cancellationToken = default)
		{
			_networkManager.SendRequest(new GameSettingsRequest());
			GameSettings gameSettings = (await _networkManager.MessageReceived
				.FirstAsync<GameSettingsResponse>(cancellationToken)).Message.GameSettings;
			return gameSettings;
		}

		public async UniTask DownloadMapUpdatesAsync(CancellationToken cancellationToken = default)
		{
			_networkManager.MessageReceived.OfMessageType<AddedVoxelResponse>()
				.Subscribe(directedMessage => _addingVoxels.AddRange(directedMessage.Message.AddedVoxels))
				.AddTo(cancellationToken);
			_networkManager.MessageReceived.OfMessageType<RemovedPositionResponse>()
				.Subscribe(directedMessage => _removingPositions.AddRange(directedMessage.Message.RemovedPositions))
				.AddTo(cancellationToken);

			while (!cancellationToken.IsCancellationRequested)
			{
				if (_mapProvider.Map != null)
				{
					_mapProvider.Map.SetVoxelsByGlobalPositions(_addingVoxels);
					_addingVoxels.Clear();

					var removingVoxels = ArrayPool<Voxel>.Shared.Rent(_removingPositions.Count);

					for (var i = 0; i < _removingPositions.Count; i++)
					{
						removingVoxels[i] = new Voxel(_removingPositions[i], VoxelData.Air);
					}

					_mapProvider.Map.SetVoxelsByGlobalPositions(removingVoxels);
					_removingPositions.Clear();

					ArrayPool<Voxel>.Shared.Return(removingVoxels);
				}

				await UniTask.Yield();
			}
		}

		private async UniTask<Map> CreateMap(string mapName, MapData mapData, IProgress<float> progress)
		{
			MapConfigure mapConfigure = _mapConfigureLoader.GetMapConfigure(mapName);
			MapBuilder mapBuilder = new MapBuilder(_mapFactory, mapData).FromConfigure(mapConfigure);
			return await mapBuilder.BuildAsync(progress, Application.exitCancellationToken);
		}
	}
}
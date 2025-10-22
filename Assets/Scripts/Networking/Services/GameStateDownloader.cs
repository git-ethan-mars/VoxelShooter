using System;
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
namespace Networking
{
	public class GameStateDownloader
	{
		private readonly VoxelShooterNetworkManager _networkManager;
		private readonly IMapConfigureLoader _mapConfigureLoader;
		private readonly IMapFactory _mapFactory;

		public GameStateDownloader(VoxelShooterNetworkManager networkManager, IMapConfigureLoader mapConfigureLoader, IMapFactory mapFactory)
		{
			_networkManager = networkManager;
			_mapConfigureLoader = mapConfigureLoader;
			_mapFactory = mapFactory;
		}

		public async UniTask<Map> DownloadMapAsync(string mapName, IProgress<float> progress, CancellationToken cancellationToken = default)
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
		
		public async UniTask<TimeSpan> DownloadGameTime(CancellationToken cancellationToken = default)
		{
			_networkManager.SendRequest(new GameTimeRequest());
			TimeSpan timeLeft = (await _networkManager.MessageReceived.FirstAsync<GameTimeResponse>(cancellationToken)).Message.TimeLeft;
			return timeLeft;
		}

		private async UniTask<Map> CreateMap(string mapName, MapData mapData, IProgress<float> progress)
		{
			MapConfigure mapConfigure = _mapConfigureLoader.GetMapConfigure(mapName);
			MapBuilder mapBuilder = new MapBuilder(_mapFactory, mapData).FromConfigure(mapConfigure);
			return await mapBuilder.BuildAsync(progress, Application.exitCancellationToken);
		}
	}
}
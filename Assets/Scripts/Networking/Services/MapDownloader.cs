using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Networking.Core;
using Networking.Messages;
using R3;
using UnityEngine;
using VoxelMap;
namespace Networking
{
	public class MapDownloader
	{
		private readonly VoxelShooterNetworkManager _networkManager;

		public MapDownloader(VoxelShooterNetworkManager networkManager)
		{
			_networkManager = networkManager;
		}

		public async UniTask<string> DownloadMapNameAsync(CancellationToken cancellationToken = default)
		{
			var request = new MapNameRequest();
			_networkManager.SendRequest(request);
			var response = await _networkManager.MessageReceived
				.OfMessageType<MapNameResponse>().FirstAsync(cancellationToken: cancellationToken).AsUniTask();
			return response.Message.MapName;
		}

		public async UniTask<MapData> DownloadMapAsync(IProgress<float> progress, CancellationToken cancellationToken = default)
		{
			var tsc = new UniTaskCompletionSource<MapData>();
			var request = new MapDownloadRequest();
			_networkManager.SendRequest(request);

			byte[] buffer = null;
			
			using IDisposable disposable = _networkManager.MessageReceived
				.OfMessageType<MapDownloadResponse>()
				.Subscribe(response => ProcessMessage(response.Message).Forget());

			async UniTask ProcessMessage(MapDownloadResponse message)
			{
				buffer ??= new byte[message.TotalBytes];
				
				Array.Copy(message.ByteChunk, 0, buffer, message.Offset, message.ByteChunk.Length);
				progress?.Report(Mathf.Clamp01((float)(message.Offset + message.ByteChunk.Length) / message.TotalBytes)); 
				if (message.Offset + message.ByteChunk.Length >= message.TotalBytes)
				{
					using var memoryStream = new MemoryStream(buffer);
					MapData mapData = await MapDataReader.ReadFromStreamAsync(memoryStream, cancellationToken);
					tsc.TrySetResult(mapData);
				}
			}	

			return await tsc.Task;
		}
	}
}
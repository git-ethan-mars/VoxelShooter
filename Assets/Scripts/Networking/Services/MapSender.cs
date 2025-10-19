using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mirror;
using Networking.Core;
using Networking.Messages;
using R3;
using VoxelMap;

namespace Networking
{
	public class MapSender
	{
		private const int MessageSize = 500 * 1024;
		private const float SendInterval = 0.01f;

		private readonly MapProvider _mapProvider;
		private readonly VoxelShooterNetworkManager _networkManager;

		public MapSender(MapProvider mapProvider, VoxelShooterNetworkManager networkManager)
		{
			_mapProvider = mapProvider;
			_networkManager = networkManager;
		}

		public void Initialize()
		{
			_networkManager.MessageReceived
				.OfMessageType<MapNameRequest>()
				.Subscribe(directedMessage => SendMapName(directedMessage.Connection))
				.AddTo(_networkManager.HostStopped);
			_networkManager.MessageReceived
				.OfMessageType<MapDownloadRequest>()
				.Subscribe(directedMessage => SendMapAsync(directedMessage.Connection, _networkManager.HostStopped).Forget())
				.AddTo(_networkManager.HostStopped);
		}

		private void SendMapName(NetworkConnectionToClient connection)
		{
			var response = new MapNameResponse(_mapProvider.MapName);
			_networkManager.SendResponse(connection, response);
		}

		private async UniTask SendMapAsync(NetworkConnectionToClient connection, CancellationToken cancellationToken)
		{
			using var snapshot = await _mapProvider.Map.SerializeAsync();
			var messages = SplitBytesIntoMessages(snapshot.ToArray());
			await SendMessagesAsync(connection, messages, cancellationToken);
		}

		private async UniTask SendMessagesAsync(NetworkConnectionToClient connection, IEnumerable<MapDownloadResponse> messages, CancellationToken cancellationToken)
		{
			foreach (MapDownloadResponse message in messages)
			{
				if (connection == null)
				{
					return;
				}
				
				_networkManager.SendResponse(connection, message);
				await UniTask.Delay(TimeSpan.FromSeconds(SendInterval), delayTiming: PlayerLoopTiming.Update, cancellationToken: cancellationToken);
			}
		}

		private IEnumerable<MapDownloadResponse> SplitBytesIntoMessages(byte[] bytes)
		{
			for (var offset = 0; offset < bytes.Length; offset += MessageSize)
			{
				int length = Math.Min(MessageSize, bytes.Length - offset);
				var chunk = new byte[length];
				Array.Copy(bytes, offset, chunk, 0, length);
				yield return new MapDownloadResponse(chunk, offset, bytes.Length);
			}
		}
	}
}
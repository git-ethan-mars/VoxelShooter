using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mirror;
using Networking.Messages;
using UnityEngine;
using VoxelMap;

namespace Networking
{
	public class GameStateSender
	{
		private const int MessageSize = 500 * 1024;
		private const float SendInterval = 0.01f;

		private readonly VoxelShooterNetworkManager _networkManager;
		private readonly MapProvider _mapProvider;

		public GameStateSender(VoxelShooterNetworkManager networkManager, MapProvider mapProvider)
		{
			_networkManager = networkManager;
			_mapProvider = mapProvider;
		}

		public void SendMapName(NetworkConnectionToClient connection)
		{
			var response = new MapNameResponse(_mapProvider.MapName);
			_networkManager.SendResponse(connection, response);
		}

		public async UniTask SendMapAsync(NetworkConnectionToClient connection, CancellationToken cancellationToken = default)
		{
			if (_mapProvider.Map == null)
			{
				Debug.Log("Map is not available for send");	
				return;
			}
			
			Debug.Log($"Start sending game state to {connection}");

			byte[] snapshot = await _mapProvider.Map.MapData.SerializeAsync();
			
			var messages = SplitBytesIntoMessages(snapshot);
			await SendMessagesAsync(connection, messages, cancellationToken);
			Debug.Log($"Sending finished successfully to {connection}");
		}

		public void SendGameTime(NetworkConnectionToClient connection, TimeSpan timeLeft)
		{
			var response = new GameTimeResponse(timeLeft);
			_networkManager.SendResponse(connection, response);
		}

		private async UniTask SendMessagesAsync(NetworkConnectionToClient connection, IEnumerable<MapDownloadResponse> messages, 
			CancellationToken cancellationToken)
		{
			string currentMapName = _mapProvider.MapName;
			
			foreach (MapDownloadResponse message in messages)
			{
				if (connection == null || currentMapName != _mapProvider.MapName)
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
using System;
using Cysharp.Threading.Tasks;
using Mirror;
using Reflex.Attributes;
using Services.ServerList;
using Steamworks;
using UnityEngine;
using VoxelMap;
namespace Networking
{
	public class VSSteamNetworkManager : VSNetworkManager
	{
		[SerializeField] private SteamLobby steamLobby;
		[SerializeField] private float serverListUpdateInterval = 5.0f;

		private IServerListService _serverList;
		private Server _server;

		private MapProvider _mapProvider;
		private CSteamID _steamLobbyID;

		[Inject]
		private void Construct(IServerListService serverList, MapProvider mapProvider)
		{
			_serverList = serverList;
			_mapProvider = mapProvider;
		}

		public override async void OnStartHost()
		{
			base.OnStartHost();

			_steamLobbyID = await steamLobby.CreateLobbyAsync(maxConnections);
			_server = await _serverList.CreateNewServerAsync(_steamLobbyID.m_SteamID, maxConnections, destroyCancellationToken);
			UpdateServerPeriodically().Forget();
		}

		public override async void OnStopHost()
		{
			base.OnStopHost();

			if (_server != null)
			{
				steamLobby.LeaveLobby(_steamLobbyID);
				await _serverList.RemoveServerAsync(_server.ServerID, destroyCancellationToken);
			}
		}

		private async UniTaskVoid UpdateServerPeriodically()
		{
			while (!destroyCancellationToken.IsCancellationRequested)
			{
				await UniTask.Delay(TimeSpan.FromSeconds(serverListUpdateInterval), cancellationToken: destroyCancellationToken);
				_server.MapName = _mapProvider.MapName ?? string.Empty;
				_server.ConnectedPlayers = NetworkServer.connections.Count;
				_server.AvailableSlots = maxConnections;
				await _serverList.UpdateServerAsync(_server, destroyCancellationToken);
			}
		}
	}
}
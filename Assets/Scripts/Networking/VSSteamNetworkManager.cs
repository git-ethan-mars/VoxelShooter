using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mirror;
using R3;
using Reflex.Attributes;
using Services;
using Steamworks;
using UnityEngine;
using VoxelMap;

namespace Networking
{
	public class VSSteamNetworkManager : VSNetworkManager
	{
		private const int MaxServerListTextLength = 32;
		private const string DefaultServerTitle = "VoxelShooter server";

		[SerializeField] private SteamLobby steamLobby;
		[SerializeField] private float serverListUpdateInterval = 5.0f;

		private IServerListService _serverList;
		private MapProvider _mapProvider;
		private CancellationTokenSource _hostCancellation;

		[Inject]
		private void Construct(IServerListService serverList, MapProvider mapProvider)
		{
			_serverList = serverList;
			_mapProvider = mapProvider;
		}

		public override void OnStartHost()
		{
			base.OnStartHost();

			_hostCancellation = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
			ListServerAsync(_hostCancellation.Token).Forget();
		}

		public override void OnStopHost()
		{
			base.OnStopHost();

			_hostCancellation?.Cancel();
			_hostCancellation?.Dispose();
			_hostCancellation = null;
		}

		// Keeps the server in the server list while the host is running: the backend drops servers
		// that were not updated within its TTL, so the update loop doubles as a heartbeat.
		private async UniTaskVoid ListServerAsync(CancellationToken cancellationToken)
		{
			CSteamID steamLobbyID = CSteamID.Nil;
			Server server = null;

			try
			{
				steamLobbyID = await steamLobby.CreateLobbyAsync(maxConnections);
				// The backend requires a map name, and the map is loaded only after the host has started.
				await _mapProvider.Map.Where(map => map != null).FirstAsync(cancellationToken: cancellationToken);

				while (true)
				{
					cancellationToken.ThrowIfCancellationRequested();

					try
					{
						server = server == null
							? await _serverList.CreateServerAsync(CreateServerCreateRequest(steamLobbyID), cancellationToken)
							: await _serverList.UpdateServerAsync(server.ID, CreateServerUpdateRequest(), cancellationToken);
					}
					catch (UnityWebRequestException exception) when (exception.ResponseCode == 404)
					{
						// The backend removed the server by TTL (e.g. after a long freeze), register it again.
						server = null;
						continue;
					}
					catch (UnityWebRequestException exception)
					{
						Debug.LogWarning($"Server list request failed: {exception.Message}");
					}

					await UniTask.Delay(TimeSpan.FromSeconds(serverListUpdateInterval), cancellationToken: cancellationToken);
				}
			}
			catch (OperationCanceledException)
			{
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			finally
			{
				if (steamLobbyID != CSteamID.Nil)
				{
					steamLobby.LeaveLobby(steamLobbyID);
				}

				if (server != null)
				{
					RemoveServerAsync(server.ID).Forget();
				}
			}
		}

		private async UniTaskVoid RemoveServerAsync(long serverID)
		{
			try
			{
				await _serverList.RemoveServerAsync(serverID, destroyCancellationToken);
			}
			catch (Exception exception) when (exception is not OperationCanceledException)
			{
				// Not critical: the backend drops the server by TTL anyway.
				Debug.LogWarning($"Failed to remove server from the server list: {exception.Message}");
			}
		}

		private ServerCreateRequest CreateServerCreateRequest(CSteamID steamLobbyID)
		{
			string title = SteamFriends.GetPersonaName();
			if (string.IsNullOrWhiteSpace(title))
			{
				title = DefaultServerTitle;
			}

			return new ServerCreateRequest(Truncate(title), GetMapName(), GetOccupiedSlots(), maxConnections, steamLobbyID.m_SteamID);
		}

		private ServerUpdateRequest CreateServerUpdateRequest()
		{
			return new ServerUpdateRequest(mapName: GetMapName(), occupiedSlots: GetOccupiedSlots());
		}

		private string GetMapName()
		{
			return Truncate(_mapProvider.Map.CurrentValue.MapName);
		}

		private int GetOccupiedSlots()
		{
			// The backend rejects occupied_slots greater than available_slots.
			return Math.Min(NetworkServer.connections.Count, maxConnections);
		}

		private static string Truncate(string text)
		{
			return text.Length <= MaxServerListTextLength ? text : text.Substring(0, MaxServerListTextLength);
		}
	}
}

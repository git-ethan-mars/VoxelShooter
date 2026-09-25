using System;
using Cysharp.Threading.Tasks;
using Mirror;
using Networking.Core;
using Networking.Messages;
using R3;
using Reflex.Attributes;
using UnityEngine;
using VoxelMap;

namespace Networking
{
	public abstract class VSNetworkManager : NetworkManager
	{
		[SerializeField] private int messageSize = 500 * 1024;
		[SerializeField] private float sendInterval = 0.5f;

		private readonly Subject<DirectedMessage> _messageReceived = new Subject<DirectedMessage>();
		private readonly Subject<NetworkConnectionToClient> _playerReady = new Subject<NetworkConnectionToClient>();
		private readonly Subject<NetworkConnectionToClient> _playerDisconnected = new Subject<NetworkConnectionToClient>();
		private MapProvider _mapProvider;

		public Observable<DirectedMessage> MessageReceived => _messageReceived;

		public Observable<Unit> ClientAuthenticated => ((VSNetworkAuthenticator)authenticator).ClientAuthenticated;

		public Observable<(NetworkConnectionToClient connection, string nickName, Texture2D avatar)> PlayerConnected
			=> ((VSNetworkAuthenticator)authenticator).OnAuthenticatedPlayer;

		public Observable<NetworkConnectionToClient> PlayerReady => _playerReady;
		public Observable<NetworkConnectionToClient> PlayerDisconnected => _playerDisconnected;

		[Inject]
		private void Construct(MapProvider mapProvider)
		{
			_mapProvider = mapProvider;
		}

		public override void OnStartHost()
		{
			base.OnStartHost();

			RegisterRequest<MapNameRequest>();
			RegisterRequest<MapDownloadRequest>();
			RegisterRequest<GameSettingsRequest>();
			RegisterRequest<ChangeGameClassRequest>();
			RegisterRequest<VoteRequest>();
			RegisterRequest<VoteCancelRequest>();
		}

		public override void OnStartClient()
		{
			base.OnStartClient();

			RegisterResponse<MapNameResponse>();
			RegisterResponse<MapDownloadResponse>();
			RegisterResponse<AddedVoxelResponse>();
			RegisterResponse<RemovedPositionResponse>();
			RegisterResponse<FallingVoxelsResponse>();
			RegisterResponse<GameSettingsResponse>();
			RegisterResponse<CharacterDiedResponse>();
			RegisterResponse<MapChangeResponse>();
			RegisterResponse<StaticAudioResponse>();
			RegisterResponse<DynamicAudioResponse>();
			RegisterResponse<VoteResponse>();
			RegisterResponse<VoteFinishResponse>();
		}

		public override void OnStopHost()
		{
			base.OnStopHost();

			UnregisterRequest<MapNameRequest>();
			UnregisterRequest<MapDownloadRequest>();
			UnregisterRequest<GameSettingsRequest>();
			UnregisterRequest<ChangeGameClassRequest>();
			UnregisterRequest<VoteRequest>();
			UnregisterRequest<VoteCancelRequest>();
		}

		public override void OnStopClient()
		{
			base.OnStopClient();

			UnregisterResponse<MapNameResponse>();
			UnregisterResponse<MapDownloadResponse>();
			UnregisterResponse<AddedVoxelResponse>();
			UnregisterResponse<RemovedPositionResponse>();
			UnregisterResponse<FallingVoxelsResponse>();
			UnregisterResponse<GameSettingsResponse>();
			UnregisterResponse<CharacterDiedResponse>();
			UnregisterResponse<MapChangeResponse>();
			UnregisterResponse<StaticAudioResponse>();
			UnregisterResponse<DynamicAudioResponse>();
			UnregisterResponse<VoteResponse>();
			UnregisterResponse<VoteFinishResponse>();
		}

		public override void OnServerDisconnect(NetworkConnectionToClient connection)
		{
			base.OnServerDisconnect(connection);

			_playerDisconnected.OnNext(connection);
		}

		public override void OnServerReady(NetworkConnectionToClient connection)
		{
			base.OnServerReady(connection);

			_playerReady.OnNext(connection);
		}

		public override void OnClientConnect()
		{
			// Base implementation calls NetworkClient.Ready() right away.
			// Client becomes ready only after the map is loaded (see GameMode and InitializeHostState).
		}

		public async UniTask SendMapAsync(NetworkConnectionToClient connection)
		{
			string currentMapName = _mapProvider.Map.CurrentValue.MapName;
			byte[] snapshot = await _mapProvider.Map.CurrentValue.MapData.SerializeAsync();

			for (int offset = 0; offset < snapshot.Length; offset += messageSize)
			{
				int length = Math.Min(messageSize, snapshot.Length - offset);
				byte[] chunk = new byte[length];
				Array.Copy(snapshot, offset, chunk, 0, length);
				var message = new MapDownloadResponse(chunk, offset, snapshot.Length);

				if (connection == null || currentMapName != _mapProvider.Map.CurrentValue.MapName)
				{
					return;
				}

				SendResponse(connection, message);
				await UniTask.Delay(TimeSpan.FromSeconds(sendInterval), delayTiming: PlayerLoopTiming.Update);
			}
		}

		public void SendRequest<TRequest>(TRequest request) where TRequest : struct, IRequest
		{
			NetworkClient.Send(request);
		}

		public void SendResponse<TResponse>(NetworkConnectionToClient connection, TResponse response) where TResponse : struct, IResponse
		{
			connection.Send(response);
		}

		public void SendResponseToAll<TResponse>(TResponse response, bool sendToReadyOnly = false) where TResponse : struct, IResponse
		{
			NetworkServer.SendToAll(response, sendToReadyOnly: sendToReadyOnly);
		}

		private void RegisterRequest<TRequest>(bool requireAuthenticate = true) where TRequest : struct, IRequest
		{
			NetworkServer.RegisterHandler<TRequest>(OnRequestReceived, requireAuthenticate);
			return;

			void OnRequestReceived(NetworkConnectionToClient connection, TRequest request)
			{
				_messageReceived.OnNext(new DirectedMessage(connection, request));
			}
		}

		private void UnregisterRequest<TRequest>() where TRequest : struct, IRequest
		{
			NetworkServer.UnregisterHandler<TRequest>();
		}

		private void RegisterResponse<TResponse>(bool requireAuthentication = true) where TResponse : struct, IResponse
		{
			NetworkClient.RegisterHandler<TResponse>(OnResponseReceived, requireAuthentication);
			return;

			void OnResponseReceived(TResponse response)
			{
				_messageReceived.OnNext(new DirectedMessage(response));
			}
		}

		private void UnregisterResponse<TResponse>() where TResponse : struct, IResponse
		{
			NetworkClient.UnregisterHandler<TResponse>();
		}
	}
}

using System.Threading;
using Cysharp.Threading.Tasks;
using Mirror;
using Networking.Core;
using Networking.Messages;
using R3;
using Reflex.Attributes;
using Services;
using UnityEngine;
using VoxelMap;

namespace Networking
{
	public class VoxelShooterNetworkManager : NetworkManager
	{
		private readonly Subject<DirectedMessage> _messageReceived = new Subject<DirectedMessage>();
		private readonly CancellationTokenSource _hostStoppedCts = new CancellationTokenSource();
		private readonly Subject<Unit> _clientStopped = new Subject<Unit>();
		
		private IServerListService _serverList;
		private IPlayerService _playerService;
		private MapProvider _mapProvider;
		private ServerInfo _serverInfo;

		public Observable<DirectedMessage> MessageReceived => _messageReceived;
		public CancellationToken HostStopped => _hostStoppedCts.Token;
		public Observable<Unit> ClientStopped => _clientStopped;

		[Inject]
		private void Construct(IServerListService serverList, IPlayerService playerService, MapProvider mapProvider)
		{
			_serverList = serverList;
			_playerService = playerService;
			_mapProvider = mapProvider;
		}

		public override async void OnStartHost()
		{
			base.OnStartHost();

			RegisterRequest<MapNameRequest>(false);
			RegisterRequest<MapDownloadRequest>(false);
			RegisterRequest<ChangeClassRequest>();
			
			await AddToServerList();
		}

		public override void OnServerDisconnect(NetworkConnectionToClient connection)
		{
			base.OnServerDisconnect(connection);
			
			_playerService.RemovePlayer(connection.connectionId);
		}

		public override async void OnStopHost()
		{
			base.OnStopHost();
			
			UnregisterRequest<MapNameRequest>();
			UnregisterRequest<MapDownloadRequest>();
			UnregisterRequest<ChangeClassRequest>();

			if (_serverInfo != null)
			{
				await _serverList.RemoveServerAsync(_serverInfo, Application.exitCancellationToken);
			}
			
			_hostStoppedCts.Cancel();
		}

		public override void OnStartClient()
		{
			base.OnStartClient();
			
			RegisterResponse<AuthenticationResponse>(false);
			RegisterResponse<MapNameResponse>(false);
			RegisterResponse<MapDownloadResponse>(false);
		}

		public override void OnStopClient()
		{
			base.OnStopClient();
			
			UnregisterResponse<AuthenticationResponse>();
			UnregisterResponse<MapNameResponse>();
			UnregisterResponse<MapDownloadResponse>();
			
			_clientStopped.OnNext(Unit.Default);
		}

		public void SendRequest<TRequest>(TRequest request) where TRequest : struct, IRequest
		{
			NetworkClient.Send(request);
		}

		public void SendResponse<TResponse>(NetworkConnectionToClient connection, TResponse response) where TResponse : struct, IResponse
		{
			connection.Send(response);
		}

		private async UniTask AddToServerList()
		{
			_serverInfo = new ServerInfo(228, "My Test Server", _mapProvider.MapName, (byte)numPlayers, (byte)maxConnections);
			await _serverList.CreateServerAsync(_serverInfo, Application.exitCancellationToken).SuppressCancellationThrow();
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
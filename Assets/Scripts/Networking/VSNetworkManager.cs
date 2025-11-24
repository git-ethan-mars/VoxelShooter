using Mirror;
using Networking.Core;
using Networking.Messages;
using R3;
using Reflex.Attributes;
using Services;
namespace Networking
{
	public abstract class VSNetworkManager : NetworkManager
	{
		private readonly Subject<DirectedMessage> _messageReceived = new Subject<DirectedMessage>();
		private readonly Subject<Unit> _clientDisconnected = new Subject<Unit>();
		private IPlayerService _playerService;

		public Observable<DirectedMessage> MessageReceived => _messageReceived;
		public Observable<Unit> ClientDisconnected => _clientDisconnected;
		
		[Inject]
		private void Construct(IPlayerService playerService)
		{
			_playerService = playerService;
		}

		public override void OnStartHost()
		{
			base.OnStartHost();

			RegisterRequest<MapNameRequest>();
			RegisterRequest<MapDownloadRequest>();
			RegisterRequest<GameSettingsRequest>();
			RegisterRequest<ChangeClassRequest>();
		}
		
		public override void OnServerDisconnect(NetworkConnectionToClient connection)
		{
			base.OnServerDisconnect(connection);
			
			_playerService.RemovePlayer(connection.connectionId);
		}
		
		public override void OnStopHost()
		{
			base.OnStopHost();

			UnregisterRequest<MapNameRequest>();
			UnregisterRequest<MapDownloadRequest>();
			UnregisterRequest<GameSettingsRequest>();
			UnregisterRequest<ChangeClassRequest>();
		}
		
		public override void OnStartClient()
		{
			base.OnStartClient();

			RegisterResponse<AuthenticationResponse>(false);
			RegisterResponse<MapNameResponse>();
			RegisterResponse<MapDownloadResponse>();
			RegisterResponse<AddedVoxelResponse>();
			RegisterResponse<RemovedPositionResponse>();
			RegisterResponse<GameSettingsResponse>();
			RegisterResponse<ChangeGameClassResponse>();
			RegisterResponse<MapChangeResponse>();
			RegisterResponse<StaticAudioResponse>();
			RegisterResponse<DynamicAudioResponse>();
		}

		public override void OnStopClient()
		{
			base.OnStopClient();

			UnregisterResponse<AuthenticationResponse>();
			UnregisterResponse<MapNameResponse>();
			UnregisterResponse<MapDownloadResponse>();
			UnregisterResponse<AddedVoxelResponse>();
			UnregisterResponse<RemovedPositionResponse>();
			UnregisterResponse<GameSettingsResponse>();
			UnregisterResponse<ChangeGameClassResponse>();
			UnregisterResponse<MapChangeResponse>();
			UnregisterResponse<StaticAudioResponse>();
			UnregisterResponse<DynamicAudioResponse>();
		}

		public override void OnClientDisconnect()
		{
			base.OnClientDisconnect();
			
			_clientDisconnected.OnNext(Unit.Default);
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
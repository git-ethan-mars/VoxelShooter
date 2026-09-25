using Mirror;
using Networking.Messages;
using R3;
using Reflex.Attributes;
using UnityEngine;

namespace Networking
{
	public class VSNetworkAuthenticator : NetworkAuthenticator
	{
		private readonly Subject<(NetworkConnectionToClient, string, Texture2D)> _onAuthenticatedPlayer =
			new Subject<(NetworkConnectionToClient, string, Texture2D)>();

		private readonly Subject<Unit> _clientAuthenticated = new Subject<Unit>();

		private VSNetworkManager _networkManager;
		private IPlayerDataLoader _playerDataLoader;

		public Observable<(NetworkConnectionToClient, string, Texture2D)> OnAuthenticatedPlayer => _onAuthenticatedPlayer;
		public Observable<Unit> ClientAuthenticated => _clientAuthenticated;

		[Inject]
		private void Construct(VSNetworkManager networkManager, IPlayerDataLoader playerDataLoader)
		{
			_networkManager = networkManager;
			_playerDataLoader = playerDataLoader;
		}

		public override void OnStartServer()
		{
			NetworkServer.RegisterHandler<AuthenticationRequest>(OnAuthenticationRequest, false);
		}

		public override void OnStartClient()
		{
			NetworkClient.RegisterHandler<AuthenticationResponse>(OnAuthenticationResponse, false);
		}

		public override void OnStopServer()
		{
			NetworkServer.UnregisterHandler<AuthenticationRequest>();
		}

		public override void OnStopClient()
		{
			NetworkClient.UnregisterHandler<AuthenticationResponse>();
		}

		public async override void OnClientAuthenticate()
		{
			base.OnClientAuthenticate();

			string nickName = _playerDataLoader.GetPlayerNickName();
			Texture2D avatar = await _playerDataLoader.GetPlayerAvatarAsync();
			var request = new AuthenticationRequest(nickName, avatar);
			_networkManager.SendRequest(request);
		}

		private void OnAuthenticationRequest(NetworkConnectionToClient connection, AuthenticationRequest request)
		{
			var response = new AuthenticationResponse();
			connection.Send(response);
			ServerAccept(connection);

			_onAuthenticatedPlayer.OnNext((connection, request.NickName, request.Avatar));
		}

		private void OnAuthenticationResponse(AuthenticationResponse response)
		{
			ClientAccept();
			_clientAuthenticated.OnNext(Unit.Default);
		}
	}
}

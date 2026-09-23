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

		private VSNetworkManager _networkManager;
		private IPlayerDataLoader _playerDataLoader;

		public Observable<(NetworkConnectionToClient, string, Texture2D)> OnAuthenticatedPlayer => _onAuthenticatedPlayer;

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

		public override void OnStopServer()
		{
			NetworkServer.UnregisterHandler<AuthenticationRequest>();
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

			if (_networkManager.mode == NetworkManagerMode.Host)
			{
				ClientAccept();
			}

			_onAuthenticatedPlayer.OnNext((connection, request.NickName, request.Avatar));
		}
	}
}

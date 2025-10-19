using Data;
using Mirror;
using Networking.Messages;
using Reflex.Attributes;
using Services;
using UnityEngine;
namespace Networking
{
	public class LocalAuthenticator : NetworkAuthenticator
	{
		private VoxelShooterNetworkManager _networkManager;
		private IPlayerService _playerService;
		private MapSender _mapSender;
		private MapDownloader _mapDownloader;
		private IPlayerDataLoader _playerDataLoader;

		[Inject]
		private void Construct(VoxelShooterNetworkManager networkManager, IPlayerService playerService, IPlayerDataLoader playerDataLoader)
		{
			_networkManager = networkManager;
			_playerService = playerService;
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
			var playerData = new PlayerData(request.NickName, request.Avatar);
			_playerService.AddPlayer(connection.connectionId, playerData);
			var response = new AuthenticationResponse();
			connection.Send(response);
			ServerAccept(connection);
		}
	}
}
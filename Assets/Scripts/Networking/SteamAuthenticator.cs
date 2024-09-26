using Mirror;
using Networking.Host;
using Networking.Messages.Requests;
using Networking.Messages.Responses;
using Steamworks;

namespace Networking
{
	public class SteamAuthenticator : NetworkAuthenticator
	{
		private IHost _host;

		public void Construct(IHost host)
		{
			_host = host;
		}
		
		public override void OnStartServer()
		{
			NetworkServer.RegisterHandler<AuthenticationRequest>(OnAuthRequestMessage, false);
		}

		public override void OnStopServer()
		{
			NetworkServer.UnregisterHandler<AuthenticationRequest>();
		}

		private void OnAuthRequestMessage(NetworkConnectionToClient connection, AuthenticationRequest message)
		{
			if (_host.AddPlayer(connection, message.Id, message.NickName))
			{
				connection.Send(new AuthenticationResponse());
				ServerAccept(connection);
				_host.SendMap(connection);
			}
			else
			{
				ServerReject(connection);
			}
		}

		public override void OnStartClient()
		{
			NetworkClient.RegisterHandler<AuthenticationResponse>(OnSuccessAuthentication);
		}

		public override void OnClientAuthenticate()
		{
			var id = SteamUser.GetSteamID().m_SteamID;
			var nickName = SteamFriends.GetPersonaName();
			NetworkClient.Send(new AuthenticationRequest(id, nickName));
		}

		public override void OnStopClient()
		{
			NetworkClient.UnregisterHandler<AuthenticationResponse>();
		}

		private void OnSuccessAuthentication(AuthenticationResponse response)
		{
			ClientAccept();
		}
	}
}
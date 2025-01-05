using Mirror;
using Networking.Messages.Requests;
using Networking.Messages.Responses;

namespace Networking.Host
{
    public partial class MirrorHost : IRequestHandler<AuthenticationRequest>
    {
        public void OnRequestReceived(NetworkConnectionToClient connection, AuthenticationRequest request)
        {
            if (AddPlayer(connection, request.Id, request.NickName))
            {
                connection.Send(new AuthenticationResponse());
                _networkManager.authenticator.OnServerAuthenticated.Invoke(connection);
                SendMap(connection);
            }
            else
            {
                connection.Disconnect();
            }
        }
    }
}
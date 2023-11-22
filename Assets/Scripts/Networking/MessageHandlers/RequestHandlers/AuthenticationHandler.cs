using Mirror;
using Networking.Messages.Requests;

namespace Networking.MessageHandlers.RequestHandlers
{
    public class AuthenticationHandler : RequestHandler<AuthenticationRequest>
    {
        private readonly IServer _server;

        public AuthenticationHandler(IServer server)
        {
            _server = server;
        }

        protected override void OnRequestReceived(NetworkConnectionToClient connection, AuthenticationRequest request)
        {
            _server.AddPlayer(connection, request.SteamID, request.NickName);
        }
    }
}
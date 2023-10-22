using Mirror;
using Networking.Messages.Requests;

namespace Networking.MessageHandlers.RequestHandlers
{
    public class ChangeClassHandler : RequestHandler<ChangeClassRequest>
    {
        private readonly IServer _server;

        public ChangeClassHandler(IServer server)
        {
            _server = server;
        }

        protected override void OnRequestReceived(NetworkConnectionToClient connection, ChangeClassRequest request)
        {
            var result = _server.ServerData.TryGetPlayerData(connection, out _);
            if (!result)
            {
                _server.AddPlayer(connection, request.GameClass, request.SteamID, request.Nickname);
            }
            else
            {
                _server.ChangeClass(connection, request.GameClass);
            }
        }
    }
}
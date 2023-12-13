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
            var playerData = _server.Data.GetPlayerData(connection);
            if (playerData.GameClass != request.GameClass)
            {
                _server.ChangeClass(connection, request.GameClass);
            }
        }
    }
}
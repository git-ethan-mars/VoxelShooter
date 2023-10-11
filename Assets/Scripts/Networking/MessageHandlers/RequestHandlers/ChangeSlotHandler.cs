using Mirror;
using Networking.Messages.Requests;
using Networking.Messages.Responses;

namespace Networking.MessageHandlers.RequestHandlers
{
    public class ChangeSlotHandler : RequestHandler<ChangeSlotRequest>
    {
        private readonly IServer _server;

        public ChangeSlotHandler(IServer server)
        {
            _server = server;
        }

        protected override void OnRequestReceived(NetworkConnectionToClient connection, ChangeSlotRequest request)
        {
            var result = _server.ServerData.TryGetPlayerData(connection, out var playerData);
            if (!result || !playerData.IsAlive) return;
            connection.identity.GetComponent<PlayerLogic.Inventory>().currentSlotId = request.Index;
            connection.Send(new ChangeSlotResponse(request.Index));
        }
    }
}
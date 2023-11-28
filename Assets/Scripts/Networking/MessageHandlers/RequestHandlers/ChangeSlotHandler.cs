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
            var result = _server.Data.TryGetPlayerData(connection, out var playerData);
            if (!result || !playerData.IsAlive)
            {
                return;
            }

            if (request.Index < 0 || request.Index >= playerData.ItemIds.Count)
            {
                return;
            }

            playerData.SelectedSlotIndex = request.Index;
            connection.Send(new ChangeSlotResponse(playerData.SelectedSlotIndex));
            NetworkServer.SendToReady(new ChangeItemModelResponse(connection.identity,
                playerData.ItemIds[playerData.SelectedSlotIndex]));
        }
    }
}
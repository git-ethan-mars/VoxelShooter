using Mirror;
using Networking.Messages.Requests;
using Networking.Messages.Responses;

namespace Networking.MessageHandlers.RequestHandlers
{
    public class DecrementSlotIndexHandler : RequestHandler<DecrementSlotIndexRequest>
    {
        private readonly IServer _server;

        public DecrementSlotIndexHandler(IServer server)
        {
            _server = server;
        }

        protected override void OnRequestReceived(NetworkConnectionToClient connection,
            DecrementSlotIndexRequest request)
        {
            var result = _server.Data.TryGetPlayerData(connection, out var playerData);
            if (!result || !playerData.IsAlive)
            {
                return;
            }

            playerData.SelectedSlotIndex = (playerData.SelectedSlotIndex - 1 + playerData.ItemIds.Count) %
                                           playerData.ItemIds.Count;
            connection.Send(new ChangeSlotResponse(playerData.SelectedSlotIndex));
            NetworkServer.SendToReady(new ChangeItemModelResponse(connection.identity,
                playerData.ItemIds[playerData.SelectedSlotIndex]));
        }
    }
}
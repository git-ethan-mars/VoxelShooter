using Mirror;
using Networking.Messages.Requests;
using Networking.Messages.Responses;
using Networking.ServerServices;

namespace Networking.MessageHandlers.RequestHandlers
{
    public class IncrementSlotIndexHandler : RequestHandler<IncrementSlotIndexRequest>
    {
        private readonly IServer _server;
        private AudioService _audioService;

        public IncrementSlotIndexHandler(IServer server, AudioService audioService)
        {
            _server = server;
            _audioService = audioService;
        }

        protected override void OnRequestReceived(NetworkConnectionToClient connection,
            IncrementSlotIndexRequest request)
        {
            var result = _server.Data.TryGetPlayerData(connection, out var playerData);
            if (!result || !playerData.IsAlive)
            {
                return;
            }

            playerData.SelectedSlotIndex = (playerData.SelectedSlotIndex + 1 + playerData.Items.Count) %
                                           playerData.Items.Count;
            connection.Send(new ChangeSlotResponse(playerData.SelectedSlotIndex));
            NetworkServer.SendToReady(new ChangeItemModelResponse(connection.identity,
                playerData.Items[playerData.SelectedSlotIndex].id));
            _audioService.StopContinuousSound(connection.identity);
        }
    }
}
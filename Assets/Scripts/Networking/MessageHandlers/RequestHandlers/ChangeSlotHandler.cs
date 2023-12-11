using Mirror;
using Networking.Messages.Requests;
using Networking.Messages.Responses;
using Networking.ServerServices;

namespace Networking.MessageHandlers.RequestHandlers
{
    public class ChangeSlotHandler : RequestHandler<ChangeSlotRequest>
    {
        private readonly IServer _server;
        private readonly AudioService _audioService;

        public ChangeSlotHandler(IServer server, AudioService audioService)
        {
            _server = server;
            _audioService = audioService;
        }

        protected override void OnRequestReceived(NetworkConnectionToClient connection, ChangeSlotRequest request)
        {
            var result = _server.Data.TryGetPlayerData(connection, out var playerData);
            if (!result || !playerData.IsAlive)
            {
                return;
            }

            if (request.Index < 0 || request.Index >= playerData.Items.Count)
            {
                return;
            }

            playerData.SelectedSlotIndex = request.Index;
            connection.Send(new ChangeSlotResponse(playerData.SelectedSlotIndex));
            NetworkServer.SendToReady(new ChangeItemModelResponse(connection.identity,
                playerData.Items[playerData.SelectedSlotIndex].id));
            if (playerData.HasContinuousSound)
            {
                _audioService.StopContinuousSound(connection.identity);
                playerData.HasContinuousSound = false;
            }
        }
    }
}
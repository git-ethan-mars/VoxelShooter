using Infrastructure.Services.Input;
using Infrastructure.Services.Storage;
using Mirror;
using Networking.Messages.Responses;
using PlayerLogic.Spectator;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class SpectatorConfigureHandler : ResponseHandler<SpectatorConfigureResponse>
    {
        private readonly IInputService _inputService;
        private readonly IStorageService _storageService;

        public SpectatorConfigureHandler(IInputService inputService, IStorageService storageService)
        {
            _inputService = inputService;
            _storageService = storageService;
        }

        protected override void OnResponseReceived(SpectatorConfigureResponse response)
        {
            var spectator = NetworkClient.connection.identity.GetComponent<SpectatorPlayer>();
            spectator.Construct(_inputService, _storageService);
        }
    }
}
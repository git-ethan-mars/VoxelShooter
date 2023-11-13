using Infrastructure.Factory;
using Infrastructure.Services.Input;
using Mirror;
using Networking.Messages.Responses;
using PlayerLogic;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class PlayerConfigureHandler : ResponseHandler<PlayerConfigureResponse>
    {
        private readonly IClient _client;
        private readonly IUIFactory _uiFactory;
        private readonly IInputService _inputService;

        public PlayerConfigureHandler(IClient client, IUIFactory uiFactory, IInputService inputService)
        {
            _client = client;
            _uiFactory = uiFactory;
            _inputService = inputService;
        }

        protected override void OnResponseReceived(PlayerConfigureResponse response)
        {
            var playerGameObject = NetworkClient.connection.identity;
            var player = playerGameObject.GetComponent<Player>();
            player.Construct(_client, _uiFactory, _inputService, response.PlaceDistance, response.ItemIds,
                response.Speed, response.JumpHeight, response.Health);
        }
    }
}
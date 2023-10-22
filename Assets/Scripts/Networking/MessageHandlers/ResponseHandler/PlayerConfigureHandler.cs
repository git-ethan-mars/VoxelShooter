using Mirror;
using Networking.Messages.Responses;
using PlayerLogic;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class PlayerConfigureHandler : ResponseHandler<PlayerConfigureResponse>
    {
        private readonly IClient _client;

        public PlayerConfigureHandler(IClient client)
        {
            _client = client;
        }

        protected override void OnResponseReceived(PlayerConfigureResponse response)
        {
            var playerGameObject = NetworkClient.connection.identity;
            var player = playerGameObject.GetComponent<Player>();
            player.Construct(_client, response.PlaceDistance, response.ItemIds);
            playerGameObject.GetComponent<PlayerMovement>().Construct(response.Speed, response.JumpMultiplier);
        }
    }
}
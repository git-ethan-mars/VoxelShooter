using System;
using Infrastructure.Factory;
using Infrastructure.Services.Input;
using Infrastructure.Services.StaticData;
using Infrastructure.Services.Storage;
using Mirror;
using Networking.Messages.Responses;
using PlayerLogic;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class PlayerConfigureHandler : ResponseHandler<PlayerConfigureResponse>
    {
        public event Action<Player> PlayerCreated;
        private readonly IClient _client;
        private readonly IParticleFactory _particleFactory;
        private readonly IUIFactory _uiFactory;
        private readonly IMeshFactory _meshFactory;
        private readonly IInputService _inputService;
        private readonly IStorageService _storageService;
        private readonly IStaticDataService _staticData;

        public PlayerConfigureHandler(IClient client, IParticleFactory particleFactory)
        {
            _client = client;
            _particleFactory = particleFactory;
        }

        protected override void OnResponseReceived(PlayerConfigureResponse response)
        {
            var playerGameObject = NetworkClient.connection.identity;
            var player = playerGameObject.GetComponent<Player>();
            player.ConstructLocalPlayer(response.PlaceDistance, response.ItemIds, response.Speed, response.JumpHeight,
                response.Health);
            _particleFactory.CreateWeatherParticle(_client.Data.MapName, player.BodyOrientation);
            PlayerCreated?.Invoke(player);
        }
    }
}
using Data;
using Infrastructure.Factory;
using Infrastructure.Services.StaticData;
using Mirror;
using Networking.Messages.Requests;
using Networking.Messages.Responses;
using Networking.ServerServices;
using UnityEngine;

namespace Networking.MessageHandlers.RequestHandlers
{
    public class RocketSpawnHandler : RequestHandler<RocketSpawnRequest>
    {
        private readonly IServer _server;
        private readonly IStaticDataService _staticData;
        private readonly IEntityFactory _entityFactory;
        private readonly IParticleFactory _particleFactory;
        private readonly AudioService _audioService;

        public RocketSpawnHandler(IServer server, IStaticDataService staticData,
            IEntityFactory entityFactory,
            IParticleFactory particleFactory, AudioService audioService)
        {
            _server = server;
            _staticData = staticData;
            _entityFactory = entityFactory;
            _particleFactory = particleFactory;
            _audioService = audioService;
        }

        protected override void OnRequestReceived(NetworkConnectionToClient connection, RocketSpawnRequest request)
        {
            var result = _server.Data.TryGetPlayerData(connection, out var playerData);
            if (!result || !playerData.IsAlive)
            {
                return;
            }

            var rocketCount = playerData.CountByItem[playerData.Items[playerData.SelectedSlotIndex]];
            if (rocketCount <= 0)
                return;
            playerData.CountByItem[playerData.Items[playerData.SelectedSlotIndex]] = rocketCount - 1;
            connection.Send(new ItemUseResponse(playerData.SelectedSlotIndex, rocketCount - 1));
            var rocketData = (RocketLauncherItem) playerData.Items[playerData.SelectedSlotIndex];
            var direction = request.Ray.direction;
            var rocket = _entityFactory.CreateRocket(request.Ray.origin + direction * 3,
                Quaternion.LookRotation(direction), _server, _particleFactory, rocketData, connection, _audioService);
            rocket.GetComponent<Rigidbody>().velocity = direction * rocketData.speed;
            NetworkServer.SendToReady(new PlayerSoundResponse(_staticData.GetAudioIndex(rocketData.reloadSound),
                connection.identity));
        }
    }
}
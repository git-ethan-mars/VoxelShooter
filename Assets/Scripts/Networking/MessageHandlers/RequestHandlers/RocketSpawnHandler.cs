using Data;
using Infrastructure;
using Infrastructure.Factory;
using Mirror;
using Networking.Messages.Requests;
using Networking.Messages.Responses;
using UnityEngine;

namespace Networking.MessageHandlers.RequestHandlers
{
    public class RocketSpawnHandler : RequestHandler<RocketSpawnRequest>
    {
        private readonly IServer _server;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly IEntityFactory _entityFactory;
        private readonly IParticleFactory _particleFactory;

        public RocketSpawnHandler(IServer server, ICoroutineRunner coroutineRunner,
            IEntityFactory entityFactory,
            IParticleFactory particleFactory)
        {
            _server = server;
            _coroutineRunner = coroutineRunner;
            _entityFactory = entityFactory;
            _particleFactory = particleFactory;
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
                Quaternion.LookRotation(direction), _server, _particleFactory, rocketData, connection);
            rocket.GetComponent<Rigidbody>().velocity = direction * rocketData.speed;
        }
    }
}
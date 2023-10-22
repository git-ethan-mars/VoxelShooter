using Data;
using Infrastructure.Factory;
using Infrastructure.Services.StaticData;
using Mirror;
using Networking.Messages.Requests;
using Networking.Messages.Responses;
using UnityEngine;

namespace Networking.MessageHandlers.RequestHandlers
{
    public class RocketSpawnHandler : RequestHandler<RocketSpawnRequest>
    {
        private readonly IServer _server;
        private readonly IStaticDataService _staticData;
        private readonly IEntityFactory _entityFactory;
        private readonly IParticleFactory _particleFactory;

        public RocketSpawnHandler(IServer server, IStaticDataService staticData, IEntityFactory entityFactory,
            IParticleFactory particleFactory)
        {
            _server = server;
            _staticData = staticData;
            _entityFactory = entityFactory;
            _particleFactory = particleFactory;
        }

        protected override void OnRequestReceived(NetworkConnectionToClient connection, RocketSpawnRequest request)
        {
            var result = _server.ServerData.TryGetPlayerData(connection, out var playerData);
            if (!result || !playerData.IsAlive) return;
            var rocketCount = playerData.ItemCountById[request.ItemId];
            if (rocketCount <= 0)
                return;
            playerData.ItemCountById[request.ItemId] = rocketCount - 1;
            connection.Send(new ItemUseResponse(request.ItemId, rocketCount - 1));
            var rocketData = (RocketLauncherItem) _staticData.GetItem(request.ItemId);
            var direction = request.Ray.direction;
            var rocket = _entityFactory.CreateRocket(request.Ray.origin + direction * 2,
                Quaternion.LookRotation(direction), _server, _particleFactory, rocketData, connection);
            rocket.GetComponent<Rigidbody>().velocity = direction * rocketData.speed;
        }
    }
}
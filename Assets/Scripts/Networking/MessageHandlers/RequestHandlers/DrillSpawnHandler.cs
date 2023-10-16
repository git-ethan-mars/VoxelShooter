using Data;
using Infrastructure.Factory;
using Infrastructure.Services.StaticData;
using Mirror;
using Networking.MessageHandlers;
using Networking.Messages.Responses;
using UnityEngine;

namespace Networking.Messages.Requests
{
    public class DrillSpawnHandler : RequestHandler<DrillSpawnRequest>
    {
        private readonly IServer _server;
        private readonly IStaticDataService _staticData;
        private readonly IEntityFactory _entityFactory;
        private readonly IParticleFactory _particleFactory;

        public DrillSpawnHandler(IServer server, IStaticDataService staticData, 
            IEntityFactory entityFactory, IParticleFactory particleFactory)
        {
            _server = server;
            _staticData = staticData;
            _entityFactory = entityFactory;
            _particleFactory = particleFactory;
        }
        
        protected override void OnRequestReceived(NetworkConnectionToClient connection, DrillSpawnRequest request)
        {
            var result = _server.ServerData.TryGetPlayerData(connection, out var playerData);
            if (!result || !playerData.IsAlive) return;
            var drillCount = playerData.ItemCountById[request.ItemId];
            if (drillCount <= 0)
                return;
            playerData.ItemCountById[request.ItemId] = drillCount - 1;
            connection.Send(new ItemUseResponse(request.ItemId, drillCount - 1));
            var drillData = (DrillItem) _staticData.GetItem(request.ItemId);
            var direction = request.Ray.direction;
            var drill = _entityFactory.CreateDrill(request.Ray.origin + direction * 2,
                Quaternion.LookRotation(direction), _server.MapProvider, _server.MapUpdater, _particleFactory, drillData, connection);
            drill.GetComponent<Rigidbody>().velocity = direction * drillData.speed;
        }
    }
}
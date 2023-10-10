using System.Collections;
using System.Collections.Generic;
using Data;
using Explosions;
using Infrastructure;
using Infrastructure.Factory;
using Infrastructure.Services.StaticData;
using Mirror;
using Networking.Messages.Requests;
using Networking.Messages.Responses;
using UnityEngine;

namespace Networking.MessageHandlers.RequestHandlers
{
    public class TntSpawnHandler : RequestHandler<TntSpawnRequest>
    {
        private readonly IServer _server;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly IEntityFactory _entityFactory;
        private readonly IStaticDataService _staticData;
        private readonly ChainExplosionBehaviour _chainExplosionBehaviour;

        public TntSpawnHandler(IServer server, ICoroutineRunner coroutineRunner, IEntityFactory entityFactory,
            IStaticDataService staticData, ChainExplosionBehaviour chainExplosionBehaviour)
        {
            _server = server;
            _coroutineRunner = coroutineRunner;
            _entityFactory = entityFactory;
            _staticData = staticData;
            _chainExplosionBehaviour = chainExplosionBehaviour;
        }

        protected override void OnRequestReceived(NetworkConnectionToClient connection, TntSpawnRequest request)
        {
            var result = _server.ServerData.TryGetPlayerData(connection, out var playerData);
            if (!result || !playerData.IsAlive) return;
            var tntCount = playerData.ItemCountById[request.ItemId];
            if (tntCount <= 0)
                return;
            var tnt = _entityFactory.CreateTnt(request.Position, request.Rotation);
            var tntData = (TntItem) _staticData.GetItem(request.ItemId);
            playerData.ItemCountById[request.ItemId] = tntCount - 1;
            _coroutineRunner.StartCoroutine(ExplodeTnt(Vector3Int.FloorToInt(request.ExplosionCenter), tnt,
                tntData.delayInSeconds,
                tntData.radius, connection, tntData.damage, tntData.particlesSpeed, tntData.particlesCount));
            connection.Send(new ItemUseResponse(request.ItemId, tntCount - 1));
        }

        private IEnumerator ExplodeTnt(Vector3Int explosionCenter, GameObject tnt, float delayInSeconds,
            int radius, NetworkConnectionToClient connection, int damage, int particlesSpeed, int particlesCount)
        {
            yield return new WaitForSeconds(delayInSeconds);
            if (!tnt) yield break;

            var explodedTnt = new List<GameObject>();
            _chainExplosionBehaviour.Explode(explosionCenter, tnt, radius, connection, damage, particlesSpeed,
                particlesCount, explodedTnt, tnt.tag);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using Data;
using Explosions;
using Infrastructure;
using Infrastructure.Factory;
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
        private readonly ExplosionBehaviour _chainExplosionBehaviour;

        public TntSpawnHandler(IServer server, CustomNetworkManager networkManager,
            ExplosionBehaviour chainExplosionBehaviour)
        {
            _server = server;
            _coroutineRunner = networkManager;
            _entityFactory = networkManager.EntityFactory;
            _chainExplosionBehaviour = chainExplosionBehaviour;
        }

        protected override void OnRequestReceived(NetworkConnectionToClient connection, TntSpawnRequest request)
        {
            var result = _server.Data.TryGetPlayerData(connection, out var playerData);
            if (!result || !playerData.IsAlive || playerData.SelectedItem is not TntItem tntData)
            {
                return;
            }

            var tntCount = playerData.CountByItem[tntData];
            if (tntCount <= 0)
            {
                return;
            }

            playerData.CountByItem[tntData] = tntCount - 1;
            connection.Send(new ItemUseResponse(playerData.SelectedSlotIndex, tntCount - 1));
            var tnt = _entityFactory.CreateTnt(request.Position, request.Rotation);
            _coroutineRunner.StartCoroutine(ExplodeTnt(Vector3Int.FloorToInt(request.ExplosionCenter), tnt,
                tntData.delayInSeconds,
                tntData.radius, connection, tntData.damage, tntData.particlesSpeed, tntData.particlesCount));
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
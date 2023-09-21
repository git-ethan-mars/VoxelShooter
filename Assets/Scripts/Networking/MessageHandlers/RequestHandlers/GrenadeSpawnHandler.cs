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
    public class GrenadeSpawnHandler : RequestHandler<GrenadeSpawnRequest>
    {
        private readonly IServer _server;
        private readonly IEntityFactory _entityFactory;
        private readonly IStaticDataService _staticData;
        private readonly SingleExplosionBehaviour _singleExplosionBehaviour;
        private readonly ICoroutineRunner _coroutineRunner;

        public GrenadeSpawnHandler(IServer server, ICoroutineRunner coroutineRunner, IEntityFactory entityFactory,
            IStaticDataService staticData, SingleExplosionBehaviour singleExplosionBehaviour)
        {
            _server = server;
            _entityFactory = entityFactory;
            _staticData = staticData;
            _singleExplosionBehaviour = singleExplosionBehaviour;
            _coroutineRunner = coroutineRunner;
        }

        protected override void OnRequestReceived(NetworkConnectionToClient connection, GrenadeSpawnRequest request)
        {
            var result = _server.ServerData.TryGetPlayerData(connection, out var playerData);
            if (!result || !playerData.IsAlive) return;
            var grenadeCount = playerData.ItemCountById[request.ItemId];
            if (grenadeCount <= 0)
                return;
            playerData.ItemCountById[request.ItemId] = grenadeCount - 1;
            connection.Send(new ItemUseResponse(request.ItemId, grenadeCount - 1));
            var grenade = _entityFactory.CreateGrenade(request.Ray.origin, Quaternion.identity);
            grenade.GetComponent<Rigidbody>().AddForce(request.Ray.direction * request.ThrowForce);
            var grenadeData = (GrenadeItem) _staticData.GetItem(request.ItemId);
            _coroutineRunner.StartCoroutine(ExplodeGrenade(grenade, grenadeData.delayInSeconds, grenadeData.radius,
                grenadeData.damage, grenadeData.particlesSpeed, grenadeData.particlesCount, connection));
        }

        private IEnumerator ExplodeGrenade(GameObject grenade, float delayInSeconds, int radius, int damage,
            int particlesSpeed, int particlesCount, NetworkConnectionToClient connection)
        {
            yield return new WaitForSeconds(delayInSeconds);
            if (!grenade) yield break;
            var position = grenade.transform.position;
            var grenadePosition = new Vector3Int((int) position.x,
                (int) position.y, (int) position.z);

            var explodedGrenades = new List<GameObject>();
            _singleExplosionBehaviour.Explode(grenadePosition, grenade, radius, connection, damage,
                particlesSpeed, particlesCount, explodedGrenades, grenade.tag);
        }
    }
}
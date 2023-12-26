using System.Collections;
using System.Collections.Generic;
using Data;
using Explosions;
using Infrastructure;
using Infrastructure.Factory;
using Mirror;
using Networking.Messages.Requests;
using Networking.Messages.Responses;
using Networking.ServerServices;
using UnityEngine;

namespace Networking.MessageHandlers.RequestHandlers
{
    public class GrenadeSpawnHandler : RequestHandler<GrenadeSpawnRequest>
    {
        private readonly IServer _server;
        private readonly IEntityFactory _entityFactory;
        private readonly ExplosionBehaviour _explosionBehaviour;
        private readonly AudioService _audioService;
        private readonly ICoroutineRunner _coroutineRunner;

        public GrenadeSpawnHandler(IServer server, CustomNetworkManager networkManager,
            ExplosionBehaviour explosionBehaviour, AudioService audioService)
        {
            _server = server;
            _entityFactory = networkManager.EntityFactory;
            _explosionBehaviour = explosionBehaviour;
            _audioService = audioService;
            _coroutineRunner = networkManager;
        }

        protected override void OnRequestReceived(NetworkConnectionToClient connection, GrenadeSpawnRequest request)
        {
            var result = _server.Data.TryGetPlayerData(connection, out var playerData);
            if (!result || !playerData.IsAlive || playerData.SelectedItem is not GrenadeItem grenadeItem)
            {
                return;
            }

            var grenadeData = (GrenadeItemData) playerData.ItemData[playerData.SelectedSlotIndex];
            if (grenadeData.Count <= 0)
            {
                return;
            }

            grenadeData.Count -= 1;
            connection.Send(new ItemUseResponse(playerData.SelectedSlotIndex, grenadeData.Count - 1));
            var grenade = _entityFactory.CreateGrenade(request.Ray.origin, Quaternion.identity,
                request.Ray.direction * request.ThrowForce);
            NetworkServer.Spawn(grenade);
            _coroutineRunner.StartCoroutine(ExplodeGrenade(grenade, grenadeItem, connection));
        }

        private IEnumerator ExplodeGrenade(GameObject grenade, GrenadeItem configure,
            NetworkConnectionToClient connection)
        {
            yield return new WaitForSeconds(configure.delayInSeconds);
            if (!grenade) yield break;
            var position = grenade.transform.position;
            var grenadePosition = new Vector3Int((int) position.x,
                (int) position.y, (int) position.z);

            var explodedGrenades = new List<GameObject>();
            _explosionBehaviour.Explode(grenadePosition, grenade, configure.radius, connection, configure.damage,
                configure.particlesSpeed, configure.particlesCount, explodedGrenades, grenade.tag);
            _audioService.SendAudio(configure.explosionSound, position);
        }
    }
}
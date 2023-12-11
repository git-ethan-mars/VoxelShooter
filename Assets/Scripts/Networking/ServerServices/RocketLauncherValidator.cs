using System.Collections;
using Data;
using Infrastructure;
using Infrastructure.Factory;
using Mirror;
using Networking.Messages.Responses;
using UnityEngine;

namespace Networking.ServerServices
{
    public class RocketLauncherValidator
    {
        private readonly IServer _server;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly IParticleFactory _particleFactory;
        private readonly AudioService _audioService;
        private readonly IEntityFactory _entityFactory;

        public RocketLauncherValidator(IServer server, ICoroutineRunner coroutineRunner, IParticleFactory particleFactory,
            AudioService audioService, IEntityFactory entityFactory)
        {
            _server = server;
            _coroutineRunner = coroutineRunner;
            _particleFactory = particleFactory;
            _audioService = audioService;
            _entityFactory = entityFactory;
        }

        public void Shoot(NetworkConnectionToClient connection, Ray ray)
        {
            var result = _server.Data.TryGetPlayerData(connection, out var playerData);
            var rocketLauncher = (RocketLauncherItem) playerData.SelectedItem;
            var rocketLauncherData = (RocketLauncherData) playerData.ItemData[playerData.SelectedSlotIndex];
            
            if (!CanShoot(rocketLauncherData))
            {
                return;
            }
            
            var direction = ray.direction;
            var rocket = _entityFactory.CreateRocket(ray.origin + direction * 3,
                Quaternion.LookRotation(direction), _server, _particleFactory, rocketLauncher, connection, _audioService);
            rocket.GetComponent<Rigidbody>().velocity = direction * rocketLauncher.speed;

            rocketLauncherData.RocketsInSlotsCount -= 1;
            connection.Send(new RocketSpawnResponse(rocketLauncherData.RocketsInSlotsCount));
        }

        public void Reload(NetworkConnectionToClient connection)
        {
            var playerData = _server.Data.GetPlayerData(connection);
            var rocketLauncher = (RocketLauncherItem) playerData.SelectedItem;
            var rocketLauncherData = (RocketLauncherData)playerData.ItemData[playerData.SelectedSlotIndex];
            
            if (!CanReload(rocketLauncher, rocketLauncherData))
            {
                return;
            }

            _coroutineRunner.StartCoroutine(ReloadInternal(connection, rocketLauncher, rocketLauncherData));
            _audioService.SendAudio(rocketLauncher.reloadSound, connection.identity);
        }

        private IEnumerator ReloadInternal(NetworkConnectionToClient connection, RocketLauncherItem configure,
            RocketLauncherData data)
        {
            data.IsReloading = true;
            var waitReloading = new WaitWithoutSlotChange(_server, connection, configure.reloadTime);
            yield return waitReloading;
            if (!waitReloading.CompletedSuccessfully)
            {
                data.IsReloading = false;
                yield break;
            }

            data.IsReloading = false;
            data.TotalRockets -= configure.rechargeableRocketsCount;
            data.RocketsInSlotsCount += configure.rechargeableRocketsCount;

            var playerData = _server.Data.GetPlayerData(connection);
            connection.Send(new RocketReloadResponse(playerData.SelectedSlotIndex, data.TotalRockets,
                data.RocketsInSlotsCount));
        }

        private bool CanReload(RocketLauncherItem configure, RocketLauncherData data)
        {
            return data.TotalRockets > 0 && !data.IsReloading && data.RocketsInSlotsCount < configure.rocketSlotsCount;
        }
        
        private bool CanShoot(RocketLauncherData data)
        {
            return !data.IsReloading && data.RocketsInSlotsCount > 0;
        }
    }
}
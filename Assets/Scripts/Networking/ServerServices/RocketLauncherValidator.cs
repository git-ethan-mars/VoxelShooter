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
        private readonly AudioService _audioService;
        private readonly IEntityFactory _entityFactory;

        public RocketLauncherValidator(IServer server, CustomNetworkManager networkManager, AudioService audioService)
        {
            _server = server;
            _coroutineRunner = networkManager;
            _entityFactory = networkManager.EntityFactory;
            _audioService = audioService;
        }

        public void Shoot(NetworkConnectionToClient connection, Ray ray)
        {
            var playerData = _server.Data.GetPlayerData(connection);
            var rocketLauncher = (RocketLauncherItem) playerData.SelectedItem;
            var rocketLauncherData = (RocketLauncherItemData) playerData.ItemData[playerData.SelectedSlotIndex];

            if (!CanShoot(rocketLauncherData))
            {
                return;
            }

            var rocket = _entityFactory.CreateRocket(ray.origin + ray.direction * 3,
                Quaternion.LookRotation(ray.direction), _server, connection,
                rocketLauncher, _audioService);
            NetworkServer.Spawn(rocket);
            rocketLauncherData.ChargedRockets -= 1;
            connection.Send(new RocketSpawnResponse(playerData.SelectedSlotIndex, rocketLauncherData.ChargedRockets));
        }

        public void Reload(NetworkConnectionToClient connection)
        {
            var playerData = _server.Data.GetPlayerData(connection);
            var rocketLauncher = (RocketLauncherItem) playerData.SelectedItem;
            var rocketLauncherData = (RocketLauncherItemData) playerData.ItemData[playerData.SelectedSlotIndex];

            if (!CanReload(rocketLauncher, rocketLauncherData))
            {
                return;
            }

            _coroutineRunner.StartCoroutine(ReloadInternal(connection, rocketLauncher, rocketLauncherData));
            _audioService.SendAudio(rocketLauncher.reloadSound, connection.identity);
        }

        private IEnumerator ReloadInternal(NetworkConnectionToClient connection, RocketLauncherItem configure,
            RocketLauncherItemData itemData)
        {
            itemData.IsReloading = true;
            var waitReloading = new WaitWithoutSlotChange(_server, connection, configure.reloadTime);
            yield return waitReloading;
            if (!waitReloading.CompletedSuccessfully)
            {
                itemData.IsReloading = false;
                yield break;
            }

            itemData.IsReloading = false;
            itemData.CarriedRockets -= configure.rechargeableRocketsCount;
            itemData.ChargedRockets += configure.rechargeableRocketsCount;

            var playerData = _server.Data.GetPlayerData(connection);
            connection.Send(new RocketReloadResponse(playerData.SelectedSlotIndex, itemData.ChargedRockets,
                itemData.CarriedRockets));
        }

        private bool CanReload(RocketLauncherItem configure, RocketLauncherItemData itemData)
        {
            return itemData.CarriedRockets > 0 && !itemData.IsReloading &&
                   itemData.ChargedRockets < configure.chargedRocketsCapacity;
        }

        private bool CanShoot(RocketLauncherItemData itemData)
        {
            return !itemData.IsReloading && itemData.ChargedRockets > 0;
        }
    }
}
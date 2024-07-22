using System.Collections;
using Data;
using Infrastructure;
using Infrastructure.Factory;
using Mirror;
using Networking.Messages.Responses;
using UnityEngine;

namespace Networking.ServerServices
{
    public class DrillValidator
    {
        private readonly IServer _server;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly AudioService _audioService;
        private readonly IEntityFactory _entityFactory;

        public DrillValidator(IServer server, CustomNetworkManager networkManager, AudioService audioService)
        {
            _server = server;
            _coroutineRunner = networkManager;
            _entityFactory = networkManager.EntityFactory;
            _audioService = audioService;
        }

        public void Shoot(NetworkConnectionToClient connection, Ray ray)
        {
            var playerData = _server.GetPlayerData(connection);
            var drillItem = (DrillItem) playerData.SelectedItem;
            var drillData = (DrillItemData) playerData.ItemData[playerData.SelectedSlotIndex];

            if (!CanShoot(drillData))
            {
                return;
            }

            var drillPosition = ray.origin + ray.direction * 3;
            var drillRotation = Quaternion.LookRotation(ray.direction);
            var drill = _entityFactory.CreateDrill(drillPosition, drillRotation, drillItem, _server, connection,
                _audioService, ray.direction);
            drillData.ChargedDrills -= 1;
            connection.Send(new DrillSpawnResponse(playerData.SelectedSlotIndex, drillData.ChargedDrills));
            _coroutineRunner.StartCoroutine(Utils.DoActionAfterDelay(() => Destroy(drill),
                drillItem.lifetime));
        }

        public void Reload(NetworkConnectionToClient connection)
        {
            var playerData = _server.GetPlayerData(connection);
            var drill = (DrillItem) playerData.SelectedItem;
            var drillData = (DrillItemData) playerData.ItemData[playerData.SelectedSlotIndex];

            if (!CanReload(drill, drillData))
            {
                return;
            }

            _coroutineRunner.StartCoroutine(ReloadInternal(connection, drill, drillData));
            _audioService.SendAudio(drill.reloadSound, connection.identity);
        }

        private IEnumerator ReloadInternal(NetworkConnectionToClient connection, DrillItem configure,
            DrillItemData itemData)
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
            itemData.Amount -= 1;
            itemData.ChargedDrills += 1;

            var playerData = _server.GetPlayerData(connection);
            connection.Send(new DrillReloadResponse(playerData.SelectedSlotIndex, itemData.ChargedDrills,
                itemData.Amount));
        }

        private bool CanReload(DrillItem configure, DrillItemData itemData)
        {
            return itemData.Amount > 0 && !itemData.IsReloading && itemData.ChargedDrills < 1;
        }

        private bool CanShoot(DrillItemData itemData)
        {
            return !itemData.IsReloading && itemData.ChargedDrills > 0;
        }

        private void Destroy(GameObject drill)
        {
            if (drill != null)
            {
                NetworkServer.Destroy(drill);
            }
        }
    }
}
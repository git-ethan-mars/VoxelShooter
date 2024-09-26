using System.Collections;
using Common.StaticData;
using Infrastructure;
using Infrastructure.Factory;
using Mirror;
using Networking.Messages.Responses;
using UnityEngine;

namespace Networking.Host.Services
{
    public class DrillValidator
    {
        private readonly IHost _host;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly IEntityFactory _entityFactory;

        public DrillValidator(IHost host, ICoroutineRunner coroutineRunner, IEntityFactory entityFactory)
        {
            _host = host;
            _coroutineRunner = coroutineRunner;
            _entityFactory = entityFactory;
        }

        public void Shoot(NetworkConnectionToClient connection, Ray ray)
        {
            var playerData = _host.GetPlayerData(connection);
            var drillLauncherData = (DrillLauncherData) playerData.ItemData[playerData.SelectedSlotIndex];

            if (!CanShoot(drillLauncherData))
            {
                return;
            }

            var drillPosition = ray.origin + ray.direction * 3;
            var drillRotation = Quaternion.LookRotation(ray.direction);
            
            var drill = _entityFactory.CreateDrill(drillPosition, drillRotation, drillLauncherData);
            drill.GetComponent<Rigidbody>().velocity = ray.direction * drillLauncherData.Speed;
            _host.SpawnEntity(drill, connection);
            
            drillLauncherData.ChargedDrills -= 1;
            connection.Send(new DrillSpawnResponse(playerData.SelectedSlotIndex, drillLauncherData.ChargedDrills));
            _coroutineRunner.StartCoroutine(Utils.DoActionAfterDelay(() => _host.UnSpawnEntity(drill),
                drillLauncherData.Lifetime));
        }

        public void Reload(NetworkConnectionToClient connection)
        {
            var playerData = _host.GetPlayerData(connection);
            var drillData = (DrillLauncherData) playerData.ItemData[playerData.SelectedSlotIndex];

            if (!CanReload(drillData))
            {
                return;
            }

            _coroutineRunner.StartCoroutine(ReloadInternal(connection, drillData));
            _host.SendAudio(drillData.ReloadSound, connection.identity);
        }

        private IEnumerator ReloadInternal(NetworkConnectionToClient connection, DrillLauncherData drillLauncherData)
        {
            drillLauncherData.IsReloading = true;
            var waitReloading = new WaitWithoutSlotChange(_host, connection, drillLauncherData.ReloadTime);
            yield return waitReloading;
            if (!waitReloading.CompletedSuccessfully)
            {
                drillLauncherData.IsReloading = false;
                yield break;
            }

            drillLauncherData.IsReloading = false;
            drillLauncherData.Amount -= 1;
            drillLauncherData.ChargedDrills += 1;

            var playerData = _host.GetPlayerData(connection);
            connection.Send(new DrillReloadResponse(playerData.SelectedSlotIndex, drillLauncherData.ChargedDrills,
                drillLauncherData.Amount));
        }

        private bool CanReload(DrillLauncherData launcherData)
        {
            return launcherData.Amount > 0 && !launcherData.IsReloading && launcherData.ChargedDrills < 1;
        }

        private bool CanShoot(DrillLauncherData launcherData)
        {
            return !launcherData.IsReloading && launcherData.ChargedDrills > 0;
        }
    }
}
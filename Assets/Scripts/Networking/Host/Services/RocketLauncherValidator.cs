using System.Collections;
using Common.StaticData;
using Infrastructure;
using Infrastructure.Factory;
using Mirror;
using Networking.Messages.Responses;
using UnityEngine;

namespace Networking.Host.Services
{
	public class RocketLauncherValidator
	{
		private readonly IHost _host;
		private readonly ICoroutineRunner _coroutineRunner;
		private readonly IEntityFactory _entityFactory;

		public RocketLauncherValidator(IHost host, ICoroutineRunner coroutineRunner, IEntityFactory entityFactory)
		{
			_host = host;
			_coroutineRunner = coroutineRunner;
			_entityFactory = entityFactory;
		}

		public void Shoot(NetworkConnectionToClient connection, Ray ray)
		{
			var playerData = _host.GetPlayerData(connection);
			var rocketLauncherData = (RocketLauncherData) playerData.ItemData[playerData.SelectedSlotIndex];

			if (!CanShoot(rocketLauncherData))
			{
				return;
			}

			var rocketPosition = ray.origin + ray.direction * 3;
			var rocketRotation = Quaternion.LookRotation(ray.direction);
			var rocket = _entityFactory.CreateRocket(rocketPosition, rocketRotation, rocketLauncherData);
			rocket.GetComponent<Rigidbody>().velocity = rocket.transform.forward * rocketLauncherData.Speed;
			rocketLauncherData.ChargedRockets -= 1;
			connection.Send(new RocketSpawnResponse(playerData.SelectedSlotIndex, rocketLauncherData.ChargedRockets));
		}

		public void Reload(NetworkConnectionToClient connection)
		{
			var playerData = _host.GetPlayerData(connection);
			var rocketLauncherData = (RocketLauncherData) playerData.SelectedItemData;

			if (!CanReload(rocketLauncherData))
			{
				return;
			}

			_coroutineRunner.StartCoroutine(ReloadInternal(connection, rocketLauncherData));
			_host.SendAudio(rocketLauncherData.ReloadSound, connection.identity);
		}

		private IEnumerator ReloadInternal(NetworkConnectionToClient connection, RocketLauncherData data)
		{
			data.IsReloading = true;
			var waitReloading = new WaitWithoutSlotChange(_host, connection, data.ReloadTime);
			yield return waitReloading;
			if (!waitReloading.CompletedSuccessfully)
			{
				data.IsReloading = false;
				yield break;
			}

			data.IsReloading = false;
			data.CarriedRockets -= data.RechargeableRocketsCount;
			data.ChargedRockets += data.RechargeableRocketsCount;

			var playerData = _host.GetPlayerData(connection);
			connection.Send(new RocketReloadResponse(playerData.SelectedSlotIndex, data.ChargedRockets,
				data.CarriedRockets));
		}

		private bool CanReload(RocketLauncherData data)
		{
			return data.CarriedRockets > 0 && !data.IsReloading &&
			       data.ChargedRockets < data.ChargedRocketsCapacity;
		}

		private bool CanShoot(RocketLauncherData data)
		{
			return !data.IsReloading && data.ChargedRockets > 0;
		}
	}
}
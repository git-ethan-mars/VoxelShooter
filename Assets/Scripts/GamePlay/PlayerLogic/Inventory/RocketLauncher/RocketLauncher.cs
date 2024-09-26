using Common.StaticData;
using UnityEngine;

namespace Inventory.RocketLauncher
{
	public class RocketLauncher : MonoBehaviour, IInventoryItem
	{
		public RocketLauncherData Data { get; private set; }

		public void Construct(IEntityFactory entityFactory, RocketLauncherData data)
		{
			Data = data;
		}
		
		public void Shoot(Ray ray)
		{
			if (!CanShoot(_data))
			{
				return;
			}

			var rocketPosition = ray.origin + ray.direction * 3;
			var rocketRotation = Quaternion.LookRotation(ray.direction);
			var rocket = _entityFactory.CreateRocket(rocketPosition, rocketRotation, _data);
			rocket.GetComponent<Rigidbody>().velocity = rocket.transform.forward * _data.Speed;
			_data.ChargedRockets -= 1;
			connection.Send(new RocketSpawnResponse(playerData.SelectedSlotIndex, _data.ChargedRockets));
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
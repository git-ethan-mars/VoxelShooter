using Common.StaticData;
using GamePlay.Entities;
using Infrastructure.Factory;
using UnityEngine;

namespace Entities
{
	public interface IEntityFactory : IPlayerFactory
	{
		TntProp CreateTnt(Vector3 position, Quaternion rotation, TntData data);

		Grenade CreateGrenade(Vector3 position, GrenadeData data);

		Tombstone CreateTombstone(Vector3 position);

		Rocket CreateRocket(Vector3 position, Quaternion rotation, RocketLauncherData data);

		LootBox CreateAmmoBox(Vector3 position, Transform parent);

		LootBox CreateHealthBox(Vector3 position, Transform parent);

		LootBox CreateBlockBox(Vector3 position, Transform parent);

		SpawnPoint CreateSpawnPoint(Vector3 position, Transform parent);

		Drill CreateDrill(Vector3 position, Quaternion rotation, DrillLauncherData data);
	}
}
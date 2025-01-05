using Common.AssetManagement;
using GamePlay.Data;
using GamePlay.Factory;
using UnityEngine;
using VoxelMap;

namespace GamePlay.Entities
{
	public class EntityFactory : IEntityFactory
	{
		private const string TntPath = "Prefabs/SpawningTnt";
		private const string GrenadePath = "Prefabs/SpawningGrenade";
		private const string RocketPath = "Prefabs/Rocket";
		private const string TombstonePath = "Prefabs/Tombstone";
		private const string SpawnPointPath = "Prefabs/Spawnpoint";
		private const string AmmoBoxPath = "Prefabs/Drops/AmmoBox";
		private const string HealthBoxPath = "Prefabs/Drops/HealthBox";
		private const string BlockBoxPath = "Prefabs/Drops/BlockBox";
		private const string DrillPath = "Prefabs/Drill";

		private readonly IAssetProvider _assets;
		private readonly IParticleFactory _particleFactory;

		public EntityFactory(IAssetProvider assets, IParticleFactory particleFactory)
		{
			_assets = assets;
			_particleFactory = particleFactory;
		}

		public SpawningTnt CreateSpawningTnt(Vector3 position, Quaternion rotation, TntData data)
		{
			var tnt = _assets.Instantiate(TntPath, position, rotation).GetComponent<SpawningTnt>();
			tnt.Construct(data);
			return tnt;
		}

		public SpawningGrenade CreateSpawningGrenade(Vector3 position)
		{
			var grenade = _assets.Instantiate(GrenadePath, position, Quaternion.identity)
				.GetComponent<SpawningGrenade>();
			return grenade;
		}

		public Tombstone CreateTombstone(Vector3 position)
		{
			var tombstone =
				_assets.Instantiate(TombstonePath, position, Quaternion.identity).GetComponent<Tombstone>();
			return tombstone;
		}

		public Rocket CreateRocket(Vector3 position, Quaternion rotation, RocketLauncherData data, MapProvider mapProvider)
		{
			var rocket = _assets.Instantiate(RocketPath, position, rotation).GetComponent<Rocket>();
			rocket.Construct(mapProvider, _particleFactory, data);
			return rocket;
		}

		public LootBox CreateAmmoBox(Vector3 position, Transform parent)
		{
			return CreateLootBox(position, parent, AmmoBoxPath);
		}

		public LootBox CreateHealthBox(Vector3 position, Transform parent)
		{
			return CreateLootBox(position, parent, HealthBoxPath);
		}

		public LootBox CreateBlockBox(Vector3 position, Transform parent)
		{
			return CreateLootBox(position, parent, BlockBoxPath);
		}

		public SpawnPoint CreateSpawnPoint(SpawnPointData spawnPointData, Transform parent)
		{
			var spawnPoint = _assets.Instantiate(SpawnPointPath,
				spawnPointData.ToVectorWithOffset(), Quaternion.identity, parent).GetComponent<SpawnPoint>();
			spawnPoint.Construct(spawnPointData);
			return spawnPoint;
		}

		private LootBox CreateLootBox(Vector3 position, Transform parent, string prefabPath)
		{
			var lootBox = _assets.Instantiate(prefabPath, position, Quaternion.identity, parent)
				.GetComponent<LootBox>();
			return lootBox;
		}

		public Drill CreateDrill(Vector3 position, Quaternion rotation, DrillLauncherData data)
		{
			var drill = _assets.Instantiate(DrillPath, position, rotation).GetComponent<Drill>();
			drill.Construct(data);
			return drill;
		}
	}
}
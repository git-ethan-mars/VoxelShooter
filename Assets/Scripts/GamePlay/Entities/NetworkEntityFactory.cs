using System;
using Data;
using Mirror;
using Reflex.Extensions;
using Reflex.Injectors;
using Services;
using UnityEngine;
namespace GamePlay
{
	public class EntityFactory : IEntityFactory
	{
		private const string TntPath = "Prefabs/Entities/SpawningTnt";
		private const string GrenadePath = "Prefabs/Entities/SpawningGrenade";
		private const string RocketPath = "Prefabs/Entities/Rocket";
		private const string TombstonePath = "Prefabs/Entities/Tombstone";
		private const string SpawnPointPath = "Prefabs/Entities/Spawnpoint";
		private const string DrillPath = "Prefabs/Entities/Drill";
		private const string AmmoBoxPath = "Prefabs/Entities/Drops/AmmoBox";
		private const string HealthBoxPath = "Prefabs/Entities/Drops/HealthBox";
		private const string BlockBoxPath = "Prefabs/Entities/Drops/BlockBox";
		private const string BuilderPath = "Prefabs/Entities/Characters/Builder";
		private const string CombatantPath = "Prefabs/Entities/Characters/Combatant";
		private const string SniperPath = "Prefabs/Entities/Characters/Sniper";
		private const string GrenadierPath = "Prefabs/Entities/Characters/Grenadier";
		private const string SpectatorPlayerPath = "Prefabs/Spectator";

		private readonly IAssetProvider _assets;

		public EntityFactory(IAssetProvider assets)
		{
			_assets = assets;
		}

		public SpawningTNT CreateSpawningTnt(Vector3 position, Quaternion rotation)
		{
			var tnt = _assets.Instantiate(TntPath, position, rotation).GetComponent<SpawningTNT>();
			NetworkServer.Spawn(tnt.gameObject);
			return tnt;
		}

		public SpawningGrenade CreateSpawningGrenade(Vector3 position)
		{
			var spawningGrenade = _assets.Instantiate(GrenadePath, position, Quaternion.identity).GetComponent<SpawningGrenade>();
			NetworkServer.Spawn(spawningGrenade.gameObject);
			return spawningGrenade;
		}

		public Tombstone CreateTombstone(Vector3 position)
		{
			var tombstone = _assets.Instantiate(TombstonePath, position, Quaternion.identity).GetComponent<Tombstone>();
			NetworkServer.Spawn(tombstone.gameObject);
			return tombstone;
		}

		public Rocket CreateRocket(Vector3 position, Quaternion rotation)
		{
			var rocket = _assets.Instantiate(RocketPath, position, rotation).GetComponent<Rocket>();
			NetworkServer.Spawn(rocket.gameObject);
			return rocket;
		}

		public Drill CreateDrill(Vector3 position, Quaternion rotation)
		{
			var drill = _assets.Instantiate(DrillPath, position, rotation).GetComponent<Drill>();
			NetworkServer.Spawn(drill.gameObject);
			return drill;
		}

		public LootBox CreateLootBox(LootBoxType type, Vector3 boxPosition)
		{
			LootBox lootBox = type switch
			{
				LootBoxType.Ammo => _assets.Instantiate(AmmoBoxPath, boxPosition, Quaternion.identity).GetComponent<LootBox>(),
				LootBoxType.Health => _assets.Instantiate(HealthBoxPath, boxPosition, Quaternion.identity).GetComponent<LootBox>(),
				LootBoxType.Block => _assets.Instantiate(BlockBoxPath, boxPosition, Quaternion.identity).GetComponent<LootBox>(),
				_ => throw new ArgumentOutOfRangeException()
			};

			NetworkServer.Spawn(lootBox.gameObject);

			return lootBox;
		}

		public SpawnPoint CreateSpawnPoint(SpawnPointData spawnPointData, Transform parent)
		{
			var spawnPoint = _assets.Instantiate(SpawnPointPath,
				spawnPointData.ToVectorWithOffset(), Quaternion.identity, parent).GetComponent<SpawnPoint>();
			GameObjectInjector.InjectSingle(spawnPoint.gameObject, spawnPoint.gameObject.scene.GetSceneContainer());
			return spawnPoint;
		}

		public Character CreateCharacter(Vector3 position, GameClass chosenClass)
		{
			Character character = chosenClass switch
			{
				GameClass.Builder => _assets.Instantiate(BuilderPath, position, Quaternion.identity).GetComponent<Character>(),
				GameClass.Combatant => _assets.Instantiate(CombatantPath, position, Quaternion.identity).GetComponent<Character>(),
				GameClass.Sniper => _assets.Instantiate(SniperPath, position, Quaternion.identity).GetComponent<Character>(),
				GameClass.Grenadier => _assets.Instantiate(GrenadierPath, position, Quaternion.identity).GetComponent<Character>(),
				_ => throw new ArgumentOutOfRangeException(nameof(chosenClass))
			};
			
			return character;
		}

		public Spectator CreateSpectator(Vector3 position)
		{
			var spectator = _assets.Instantiate(SpectatorPlayerPath, position, Quaternion.identity).GetComponent<Spectator>();
			return spectator;
		}
	}
}
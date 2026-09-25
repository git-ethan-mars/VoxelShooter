using System;
using Data;
using Mirror;
using Services;
using UnityEngine;
using VoxelMap.Data;

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
		private const string SpectatorPlayerPath = "Prefabs/Entities/Spectator";
		private const string DummyPath = "Prefabs/Entities/Characters/Dummy";

		private readonly IAssetProvider _assets;
		private readonly IStaticDataService _staticData;
		private readonly IItemFactory _itemFactory;

		public EntityFactory(IAssetProvider assets, IStaticDataService staticData, IItemFactory itemFactory)
		{
			_assets = assets;
			_staticData = staticData;
			_itemFactory = itemFactory;
		}

		public SpawningTNT CreateSpawningTnt(Vector3 position, Quaternion rotation, NetworkConnectionToClient connection)
		{
			SpawningTNT tnt = _assets.Instantiate(TntPath, position, rotation).GetComponent<SpawningTNT>();
			NetworkServer.Spawn(tnt.gameObject, connection);
			return tnt;
		}

		public SpawningGrenade CreateSpawningGrenade(Vector3 position, NetworkConnectionToClient connection)
		{
			SpawningGrenade spawningGrenade = _assets.Instantiate(GrenadePath, position, Quaternion.identity).GetComponent<SpawningGrenade>();
			NetworkServer.Spawn(spawningGrenade.gameObject, connection);
			return spawningGrenade;
		}

		public Tombstone CreateTombstone(Vector3 position, NetworkConnectionToClient connection)
		{
			Tombstone tombstone = _assets.Instantiate(TombstonePath, position, Quaternion.identity).GetComponent<Tombstone>();
			NetworkServer.Spawn(tombstone.gameObject, connection);
			return tombstone;
		}

		public Rocket CreateRocket(Vector3 position, Quaternion rotation, NetworkConnectionToClient connection)
		{
			Rocket rocket = _assets.Instantiate(RocketPath, position, rotation).GetComponent<Rocket>();
			NetworkServer.Spawn(rocket.gameObject, connection);
			return rocket;
		}

		public Drill CreateDrill(Vector3 position, Quaternion rotation, NetworkConnectionToClient connection)
		{
			Drill drill = _assets.Instantiate(DrillPath, position, rotation).GetComponent<Drill>();
			NetworkServer.Spawn(drill.gameObject, connection);
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
			SpawnPoint spawnPoint = _assets.Instantiate(SpawnPointPath,
				spawnPointData.ToVectorWithOffset(), Quaternion.identity, parent).GetComponent<SpawnPoint>();
			NetworkServer.Spawn(spawnPoint.gameObject);
			return spawnPoint;
		}

		public Character CreateCharacter(Vector3 position, GameClass chosenClass, string nickName)
		{
			Character character = chosenClass switch
			{
				GameClass.Builder => _assets.Instantiate(BuilderPath, position, Quaternion.identity).GetComponent<Character>(),
				GameClass.Combatant => _assets.Instantiate(CombatantPath, position, Quaternion.identity).GetComponent<Character>(),
				GameClass.Sniper => _assets.Instantiate(SniperPath, position, Quaternion.identity).GetComponent<Character>(),
				GameClass.Grenadier => _assets.Instantiate(GrenadierPath, position, Quaternion.identity).GetComponent<Character>(),
				_ => throw new ArgumentOutOfRangeException(nameof(chosenClass))
			};

			Characteristics characteristics = _staticData.GetCharacteristics(chosenClass);
			character.Initialize(chosenClass, nickName);
			character.HealthSystem.Initialize(characteristics.MaxHealth);
			character.Inventory.Initialize(_itemFactory.CreateItems(chosenClass), characteristics.VoxelsCount);

			return character;
		}

		public Spectator CreateSpectator(Vector3 position)
		{
			Spectator spectator = _assets.Instantiate(SpectatorPlayerPath, position, Quaternion.identity).GetComponent<Spectator>();
			return spectator;
		}

		public Dummy CreateDummy(Vector3 position, Quaternion rotation, int maxHealth)
		{
			Dummy dummy = _assets.Instantiate(DummyPath, position, rotation).GetComponent<Dummy>();
			dummy.Initialize(maxHealth);
			NetworkServer.Spawn(dummy.gameObject);
			return dummy;
		}
	}
}

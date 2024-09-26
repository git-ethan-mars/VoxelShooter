using Common.AssetManagement;
using Common.Audio;
using Common.Factory;
using Common.Input;
using Common.StaticData;
using Common.Storage;
using Entities;
using Entities.PlayerLogic;
using GamePlay.Entities;
using PlayerLogic;
using PlayerLogic.Spectator;
using UnityEngine;

namespace Infrastructure.Factory
{
	public class EntityFactory : IEntityFactory
	{
		private readonly IAssetProvider _assets;
		private readonly IStorageService _storageService;
		private readonly IInputService _inputService;
		private readonly IStaticDataService _staticData;
		private readonly IMeshFactory _meshFactory;
		private readonly IParticleFactory _particleFactory;
		private readonly IAudioPlayer _audioPlayer;

		public EntityFactory(IAssetProvider assets, IStorageService storageService,
			IInputService inputService,
			IStaticDataService staticData, IMeshFactory meshFactory, IParticleFactory particleFactory, IAudioPlayer audioPlayer)
		{
			_assets = assets;
			_storageService = storageService;
			_inputService = inputService;
			_staticData = staticData;
			_meshFactory = meshFactory;
			_particleFactory = particleFactory;
			_audioPlayer = audioPlayer;
		}

		public Character CreateCharacter(Vector3 position)
		{
			var character = _assets.Instantiate(EntityPath.MainPlayerPath, position, Quaternion.identity).GetComponent<Character>();
			character.Construct(_inputService, _storageService, _staticData);
			return character;
		}

		public SpectatorPlayer CreateSpectatorPlayer(Vector3 position)
		{
			var spectator =
				_assets.Instantiate(EntityPath.SpectatorPlayerPath, position, Quaternion.identity).GetComponent<SpectatorPlayer>();
			spectator.Construct(_inputService, _storageService);
			return spectator;
		}

		public TntProp CreateTnt(Vector3 position, Quaternion rotation, TntData data)
		{
			var tnt = _assets.Instantiate(EntityPath.TntPath, position, rotation).GetComponent<TntProp>();
			tnt.Construct(_particleFactory, _audioPlayer, data);
			return tnt;
		}

		public Grenade CreateGrenade(Vector3 position, GrenadeData grenadeData)
		{
			var grenade = _assets.Instantiate(EntityPath.GrenadePath, position, Quaternion.identity)
				.GetComponent<Grenade>();
			grenade.Construct(_particleFactory, _audioPlayer, grenadeData);
			return grenade;
		}

		public Tombstone CreateTombstone(Vector3 position)
		{
			var tombstone =
				_assets.Instantiate(EntityPath.TombstonePath, position, Quaternion.identity).GetComponent<Tombstone>();
			tombstone.Construct(_particleFactory);
			return tombstone;
		}

		public Rocket CreateRocket(Vector3 position, Quaternion rotation, RocketLauncherData data)
		{
			var rocket = _assets.Instantiate(EntityPath.RocketPath, position, rotation).GetComponent<Rocket>();
			rocket.Construct(_particleFactory, _audioPlayer, data);
			return rocket;
		}

		public LootBox CreateAmmoBox(Vector3 position, Transform parent)
		{
			return CreateLootBox(position, parent, EntityPath.AmmoBoxPath);
		}

		public LootBox CreateHealthBox(Vector3 position, Transform parent)
		{
			return CreateLootBox(position, parent, EntityPath.HealthBoxPath);
		}

		public LootBox CreateBlockBox(Vector3 position, Transform parent)
		{
			return CreateLootBox(position, parent, EntityPath.BlockBoxPath);
		}

		public SpawnPoint CreateSpawnPoint(SpawnPointData spawnPointData, Transform parent)
		{
			var spawnPoint = _assets.Instantiate(EntityPath.SpawnPointPath,
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
			var drill = _assets.Instantiate(EntityPath.DrillPath, position, rotation).GetComponent<Drill>();
			drill.Construct(_audioPlayer, data);
			return drill;
		}
	}
}
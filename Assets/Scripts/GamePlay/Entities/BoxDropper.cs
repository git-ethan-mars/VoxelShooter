using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GamePlay.Data;
using UnityEngine;
using VoxelMap;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace GamePlay.Entities
{
	public class BoxDropper
	{
		private const string LootBoxContainer = "LootBoxContainer";

		private readonly IEntityFactory _entityFactory;
		private readonly MapProvider _mapProvider;
		private readonly int _spawnRate;
		private readonly Transform _parent;
		private readonly List<Vector3Int> _lootBoxSpawnPositions;

		public BoxDropper(IEntityFactory entityFactory, WorldSettings worldSettings,
			MapProvider mapProvider)
		{
			_mapProvider = mapProvider;
			_parent = new GameObject(LootBoxContainer).transform;
			_spawnRate = worldSettings.BoxSpawnTime;
			_entityFactory = entityFactory;
			_lootBoxSpawnPositions = GetLootBoxSpawnPositions();
		}

		public void Start(CancellationToken token)
		{
			SpawnLootBoxAsync(token).Forget();
		}

		private List<Vector3Int> GetLootBoxSpawnPositions()
		{
			var spawnPositions = new List<Vector3Int>();
			for (var x = 0; x < _mapProvider.Width; x++)
			{
				for (var z = 0; z < _mapProvider.Depth; z++)
				{
					if (_mapProvider.GetVoxelByGlobalPosition(x, 1, z).IsSolid())
					{
						spawnPositions.Add(new Vector3Int(x, _mapProvider.Height - 1, z));
					}
				}
			}

			return spawnPositions;
		}

		// If build up top layer, game will go into infinite loop
		private async UniTaskVoid SpawnLootBoxAsync(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				var spawnPosition = _lootBoxSpawnPositions[Random.Range(0, _lootBoxSpawnPositions.Count - 1)];
				var spawnBlock = _mapProvider.GetVoxelByGlobalPosition(spawnPosition.x,
					spawnPosition.y, spawnPosition.z);
				if (spawnBlock.IsSolid())
					continue;
				var spawnCoordinates = spawnPosition + Map.WorldOffset;
				LootBox lootBox;
				var lootBoxType = (LootBoxType) Random.Range(0, Enum.GetNames(typeof(LootBoxType)).Length);
				lootBox = lootBoxType switch
				{
					LootBoxType.Ammo => _entityFactory.CreateAmmoBox(spawnCoordinates, _parent),
					LootBoxType.Health => _entityFactory.CreateHealthBox(spawnCoordinates, _parent),
					LootBoxType.Block => _entityFactory.CreateBlockBox(spawnCoordinates, _parent),
					_ => throw new ArgumentOutOfRangeException()
				};

				lootBox.Construct();
				lootBox.PickedUp += OnPickedUp;
				await UniTask.WaitForSeconds(_spawnRate, cancellationToken:token);
			}
		}

		private void OnPickedUp(LootBox lootBox, Character character)
		{
			lootBox.PickedUp -= OnPickedUp;
			Object.Destroy(lootBox.gameObject);
		}
	}
}
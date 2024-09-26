using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Common;
using Common.StaticData;
using Cysharp.Threading.Tasks;
using Entities;
using Entities.PlayerLogic;
using Mirror;
using UnityEngine;
using VoxelMap;
using Random = UnityEngine.Random;

namespace Networking.Host.Services
{
	public class BoxDropper
	{
		private const string LootBoxContainer = "LootBoxContainer";

		private readonly IEntityFactory _entityFactory;
		private readonly IHost _host;
		private readonly MapProvider _mapProvider;
		private readonly int _spawnRate;
		private readonly Transform _parent;
		private readonly List<Vector3Int> _lootBoxSpawnPositions;

		public BoxDropper(IHost host, WorldSettings worldSettings, MapProvider mapProvider,
			IEntityFactory entityFactory)
		{
			_host = host;
			_mapProvider = mapProvider;
			_parent = new GameObject(LootBoxContainer).transform;
			_spawnRate = worldSettings.BoxSpawnTime;
			_entityFactory = entityFactory;
			_lootBoxSpawnPositions = GetLootBoxSpawnPositions();
		}

		public void Start()
		{
			_coroutine = SpawnLootBox();
			_coroutineRunner.StartCoroutine(_coroutine);
		}

		public void Stop()
		{
			_coroutineRunner.StopCoroutine(_coroutine);
		}

		private List<Vector3Int> GetLootBoxSpawnPositions()
		{
			var spawnPositions = new List<Vector3Int>();
			for (var x = 0; x < _mapProvider.Width; x++)
			{
				for (var z = 0; z < _mapProvider.Depth; z++)
				{
					if (_mapProvider.GetBlockByGlobalPosition(x, 1, z).IsSolid())
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
				var spawnBlock = _mapProvider.GetBlockByGlobalPosition(spawnPosition.x,
					spawnPosition.y, spawnPosition.z);
				if (spawnBlock.IsSolid())
					continue;
				var spawnCoordinates = spawnPosition + Constants.worldOffset;
				LootBox lootBox;
				var lootBoxType = (LootBoxType) Random.Range(0, Enum.GetNames(typeof(LootBoxType)).Length);
				switch (lootBoxType)
				{
					case LootBoxType.Ammo:
						lootBox = _entityFactory.CreateAmmoBox(spawnCoordinates, _parent);
						break;
					case LootBoxType.Health:
						lootBox = _entityFactory.CreateHealthBox(spawnCoordinates, _parent);
						break;
					default:
						lootBox = _entityFactory.CreateBlockBox(spawnCoordinates, _parent);
						break;
				}

				lootBox.Construct(_host);
				_host.SpawnEntity(lootBox);
				lootBox.PickedUp += OnPickedUp;
				await UniTask.WaitForSeconds(_spawnRate, cancellationToken:token);
			}
		}

		private void OnPickedUp(LootBox lootBox, Character character)
		{
			lootBox.PickedUp -= OnPickedUp;
			_host.UnSpawnEntity(lootBox);
		}
	}
}
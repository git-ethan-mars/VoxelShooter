using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using R3;
using UnityEngine;
using VoxelMap;
using Random = UnityEngine.Random;

namespace GamePlay
{
	public class LootBoxDropper
	{
		private readonly IEntityFactory _entityFactory;
		private readonly MapProvider _mapProvider;

		public LootBoxDropper(IEntityFactory entityFactory, MapProvider mapProvider)
		{
			_entityFactory = entityFactory;
			_mapProvider = mapProvider;
		}

		public async UniTask StartDropping(TimeSpan spawnInterval, CancellationToken token)
		{
			HashSet<Vector2Int> busyPositions = new HashSet<Vector2Int>();

			while (!token.IsCancellationRequested)
			{
				if (_mapProvider.Map == null || !_mapProvider.Map.TryGetRandomTopVoxelPosition(out Vector3Ushort topVoxelPosition) ||
				    topVoxelPosition.y == 0 || busyPositions.Contains(Vector2Int.FloorToInt(new Vector2(topVoxelPosition.x, topVoxelPosition.z))))
				{
					await UniTask.Yield(token);
				}
				else
				{
					Vector2Int gridPosition = new Vector2Int(topVoxelPosition.x, topVoxelPosition.z);
					busyPositions.Add(gridPosition);
					var boxPosition = new Vector3(topVoxelPosition.x + Map.WorldOffset.x, _mapProvider.Map.Height - 1,
						topVoxelPosition.z + Map.WorldOffset.z);
					var lootBoxType = (LootBoxType)Random.Range(0, Enum.GetNames(typeof(LootBoxType)).Length);
					LootBox lootBox = _entityFactory.CreateLootBox(lootBoxType, boxPosition);
					lootBox.PickedUp.Subscribe(_ => busyPositions.Remove(gridPosition)).AddTo(lootBox);
					await UniTask.Delay(spawnInterval, cancellationToken: token);
				}
			}
		}
	}
}
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Data;
using UnityEngine;
using VoxelMap;
using VoxelMap.Data;
namespace GamePlay
{
	public class SpawnPointService : ISpawnPointService
	{
		private readonly List<SpawnPoint> _spawnPoints = new List<SpawnPoint>();

		private readonly IEntityFactory _entityFactory;
		private readonly MapProvider _mapProvider;

		private int _spawnPointIndex;

		public SpawnPointService(IEntityFactory entityFactory, MapProvider mapProvider)
		{
			_entityFactory = entityFactory;
			_mapProvider = mapProvider;
		}

		public void CreateSpawnPoints()
		{
			_spawnPointIndex = 0;
			_spawnPoints.Clear();

			MapConfigure mapConfigure = _mapProvider.Map.MapConfigure;
			
			foreach (SpawnPointData spawnPointData in mapConfigure.SpawnPoints)
			{
				SpawnPoint spawnPoint = _entityFactory.CreateSpawnPoint(spawnPointData, _mapProvider.Map.transform);
				_spawnPoints.Add(spawnPoint);
			}
		}

		public async UniTask<Vector3> GetSpawnPointAsync()
		{
			Vector3 spawnPosition;
			
			if (_spawnPoints.Count == 0)
			{
				Vector3Ushort topVoxelPosition;
				
				while (!_mapProvider.Map.TryGetRandomTopVoxelPosition(out topVoxelPosition))
				{
					await UniTask.Yield();
				}

				spawnPosition = topVoxelPosition + Map.WorldOffset;
			}
			else
			{
				spawnPosition = _spawnPoints[_spawnPointIndex].transform.position;
				_spawnPointIndex = (_spawnPointIndex + 1 +_spawnPoints.Count) % _spawnPoints.Count;
			}
			
			return spawnPosition;
		}
	}

}
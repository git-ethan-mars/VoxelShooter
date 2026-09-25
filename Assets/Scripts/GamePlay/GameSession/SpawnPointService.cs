using System.Collections.Generic;
using UnityEngine;
using VoxelMap;
using VoxelMap.Data;

namespace GamePlay
{
	public class SpawnPointService
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
			Clear();

			foreach (SpawnPointData spawnPointData in _mapProvider.Map.CurrentValue.MapConfigure.SpawnPoints)
			{
				SpawnPoint spawnPoint = _entityFactory.CreateSpawnPoint(spawnPointData, _mapProvider.Map.CurrentValue.transform);
				_spawnPoints.Add(spawnPoint);
			}
		}

		public void Clear()
		{
			_spawnPointIndex = 0;
			_spawnPoints.Clear();
		}

		public Vector3 GetSpawnPoint()
		{
			Vector3 spawnPosition;

			if (_spawnPoints.Count == 0)
			{
				spawnPosition = Map.WorldOffset;
			}
			else
			{
				spawnPosition = _spawnPoints[_spawnPointIndex].transform.position;
				_spawnPointIndex = (_spawnPointIndex + 1 + _spawnPoints.Count) % _spawnPoints.Count;
			}

			return spawnPosition;
		}

		public Vector3 GetRandomSpawnPoint()
		{
			Vector3 spawnPosition;

			if (_spawnPoints.Count == 0)
			{
				spawnPosition = Map.WorldOffset;
			}
			else
			{
				spawnPosition = _spawnPoints[Random.Range(0, _spawnPoints.Count)].transform.position;
			}

			return spawnPosition;
		}
	}
}

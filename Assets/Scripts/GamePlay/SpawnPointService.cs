using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;
using VoxelMap;
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
			foreach (SpawnPointData spawnPointData in _mapProvider.Confgure.SpawnPoints)
			{
				SpawnPoint spawnPoint = _entityFactory.CreateSpawnPoint(spawnPointData, _mapProvider.Map.transform);
				_spawnPoints.Add(spawnPoint);
			}
		}

		public Vector3 GetSpawnPoint()
		{
			if (_spawnPoints.Count == 0)
			{
				return Vector3.zero;
			}
			
			Vector3 position = _spawnPoints[_spawnPointIndex].transform.position;
			_spawnPointIndex++;
			return position;
		}
	}

}
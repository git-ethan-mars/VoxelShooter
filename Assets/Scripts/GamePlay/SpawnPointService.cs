using System.Collections.Generic;
using Data;
using Services;
using UnityEngine;
using VoxelMap;
namespace GamePlay
{
	public class SpawnPointService : ISpawnPointService
	{
		private readonly List<SpawnPoint> _spawnPoints = new List<SpawnPoint>();

		private readonly IEntityFactory _entityFactory;
		private readonly MapProvider _mapProvider;
		private readonly IMapConfigureLoader _mapConfigureLoader;

		private int _spawnPointIndex;

		public SpawnPointService(IEntityFactory entityFactory, MapProvider mapProvider, IMapConfigureLoader mapConfigureLoader)
		{
			_entityFactory = entityFactory;
			_mapProvider = mapProvider;
			_mapConfigureLoader = mapConfigureLoader;
		}

		public void CreateSpawnPoints()
		{
			_spawnPointIndex = 0;
			_spawnPoints.Clear();

			MapConfigure mapConfigure = _mapConfigureLoader.GetMapConfigure(_mapProvider.MapName);
			
			foreach (SpawnPointData spawnPointData in mapConfigure.SpawnPoints)
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
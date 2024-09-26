using System.Collections.Generic;
using Common.StaticData;
using Entities;
using UnityEngine;

namespace Networking.Host.Services
{
	public class SpawnPointService
	{
		private const string SpawnPointContainerName = "SpawnPointContainer";
		
		private readonly WorldSettings _worldSettings;
		private readonly IStaticDataService _staticData;
		private int _spawnPointIndex;
		private readonly IEntityFactory _entityFactory;
		private List<SpawnPoint> _spawnPoints;

		public SpawnPointService(WorldSettings worldSettings, IStaticDataService staticData, IEntityFactory entityFactory)
        {
        	_worldSettings = worldSettings;
			_staticData = staticData;
            _entityFactory = entityFactory;
        }

        public void CreateSpawnPoints()
        {
	        var mapConfigure = _staticData.GetMapConfigure(_worldSettings.MapName);
            var parent = new GameObject(SpawnPointContainerName).transform;
            foreach (var spawnPointData in mapConfigure.spawnPoints)
            {
                var spawnPoint = _entityFactory.CreateSpawnPoint(spawnPointData.position, parent);
                _spawnPoints.Add(spawnPoint);
            }
        }

		public Vector3 GetSpawnPosition()
		{
			var spawnPosition = _spawnPoints[_spawnPointIndex].transform.position;
			_spawnPointIndex = (_spawnPointIndex + 1) % _spawnPoints.Count;
			return spawnPosition;
		}
	}
}
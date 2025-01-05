using System.Collections.Generic;
using GamePlay.Data;
using GamePlay.Entities;
using UnityEngine;
using VoxelMap;

namespace GamePlay
{
    public class CharacterSpawner
    {
        private const string SpawnPointContainerName = "SpawnPointContainer";

        private readonly IEntityFactory _entityFactory;
        private readonly IMapConfigureLoader _mapConfigureLoader;
        private readonly ICharacterFactory _characterFactory;
        private readonly WorldSettings _worldSettings;

        private int _spawnPointIndex;
        private List<SpawnPoint> _spawnPoints;

        public CharacterSpawner(ICharacterFactory characterFactory, IEntityFactory entityFactory,
            IMapConfigureLoader mapConfigureLoader, WorldSettings worldSettings)
        {
            _characterFactory = characterFactory;
            _worldSettings = worldSettings;
            _mapConfigureLoader = mapConfigureLoader;
            _entityFactory = entityFactory;
        }

        public Character SpawnCharacter()
        {
            _spawnPoints ??= CreateSpawnPoints();
            var nextSpawnPoint = _spawnPoints[_spawnPointIndex].transform.position;
            _spawnPointIndex = (_spawnPointIndex + 1) % _spawnPoints.Count;
            var character = _characterFactory.CreateCharacter(nextSpawnPoint);
            return character;
        }

        private List<SpawnPoint> CreateSpawnPoints()
        {
            var mapConfigure = _mapConfigureLoader.GetMapConfigure(_worldSettings.MapName);
            var parent = new GameObject(SpawnPointContainerName).transform;
            var spawnPoints = new List<SpawnPoint>();
            foreach (var spawnPointData in mapConfigure.SpawnPoints)
            {
                var spawnPoint = _entityFactory.CreateSpawnPoint(spawnPointData, parent);
                spawnPoints.Add(spawnPoint);
            }

            return spawnPoints;
        }
    }
}
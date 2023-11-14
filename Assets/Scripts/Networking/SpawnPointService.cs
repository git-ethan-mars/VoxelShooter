using System.Collections.Generic;
using Entities;
using Infrastructure.Factory;
using MapLogic;
using Networking.ServerServices;
using UnityEngine;

namespace Networking
{
    public class SpawnPointService
    {
        private const string SpawnPointContainerName = "SpawnPointContainer";

        private readonly List<SpawnPoint> _spawnPoints;
        private readonly MapProvider _mapProvider;
        private readonly IGameFactory _gameFactory;
        private readonly IEntityFactory _entityFactory;
        private readonly EntityPositionValidator _entityPositionValidator;
        private int _spawnPointIndex;

        public SpawnPointService(MapProvider mapProvider, IGameFactory gameFactory,
            IEntityFactory entityFactory, EntityPositionValidator entityPositionValidator)
        {
            _mapProvider = mapProvider;
            _gameFactory = gameFactory;
            _entityFactory = entityFactory;
            _entityPositionValidator = entityPositionValidator;
            _spawnPoints = new List<SpawnPoint>();
        }

        public void CreateSpawnPoints()
        {
            var parent = _gameFactory.CreateGameObjectContainer(SpawnPointContainerName).transform;
            foreach (var spawnPointData in _mapProvider.SceneData.SpawnPoints)
            {
                var spawnPoint = _entityFactory.CreateSpawnPoint(spawnPointData.ToVectorWithOffset(), parent)
                    .GetComponent<SpawnPoint>();
                spawnPoint.Construct(spawnPointData);
                _entityPositionValidator.AddEntity(spawnPoint);
                _spawnPoints.Add(spawnPoint);
            }
        }

        public Vector3 GetSpawnPosition()
        {
            var spawnPointData = _spawnPoints[_spawnPointIndex].Data;
            _spawnPointIndex = (_spawnPointIndex + 1) % _spawnPoints.Count;
            return spawnPointData.ToVectorWithOffset();
        }

        public void RemoveSpawnPoints()
        {
            foreach (var spawnPoint in _spawnPoints)
            {
                _entityPositionValidator.RemoveEntity(spawnPoint);
            }

            _spawnPoints.Clear();
        }
    }
}
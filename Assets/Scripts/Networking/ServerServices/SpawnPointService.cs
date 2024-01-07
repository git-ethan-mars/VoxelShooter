using System.Collections.Generic;
using Entities;
using Infrastructure.Factory;
using MapLogic;
using UnityEngine;

namespace Networking.ServerServices
{
    public class SpawnPointService
    {
        private const string SpawnPointContainerName = "SpawnPointContainer";

        private readonly IServer _server;
        private readonly List<SpawnPoint> _spawnPoints;
        private readonly MapProvider _mapProvider;
        private readonly IGameFactory _gameFactory;
        private readonly IEntityFactory _entityFactory;
        private readonly EntityPositionValidator _entityPositionValidator;
        private int _spawnPointIndex;

        public SpawnPointService(IServer server, IGameFactory gameFactory, IEntityFactory entityFactory)
        {
            _server = server;
            _mapProvider = server.MapProvider;
            _gameFactory = gameFactory;
            _entityFactory = entityFactory;
            _spawnPoints = new List<SpawnPoint>();
        }

        public void CreateSpawnPoints()
        {
            var parent = _gameFactory.CreateGameObjectContainer(SpawnPointContainerName);
            foreach (var spawnPointData in _mapProvider.SceneData.SpawnPoints)
            {
                var spawnPoint = _entityFactory.CreateSpawnPoint(spawnPointData.ToVectorWithOffset(), parent);
                spawnPoint.Construct(spawnPointData);
                _server.EntityContainer.AddPushable(spawnPoint);
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
                _server.EntityContainer.RemovePushable(spawnPoint);
            }

            _spawnPoints.Clear();
        }
    }
}
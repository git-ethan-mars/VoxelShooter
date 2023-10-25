using System.Collections.Generic;
using CameraLogic;
using Data;
using Infrastructure.AssetManagement;
using Networking;
using UnityEngine;

namespace Infrastructure.Factory
{
    public class PlayerFactory : IPlayerFactory
    {
        private readonly List<SpawnPointData> _spawnPoints;
        private readonly IAssetProvider _assets;
        private readonly IServer _server;
        private int _spawnPointIndex;
        private const string PlayerPath = "Prefabs/Player";
        private const string SpectatorPlayerPath = "Prefabs/Spectator player";

        public PlayerFactory(IServer server,
            IAssetProvider assets)
        {
            _server = server;
            _spawnPoints = _server.MapProvider.SceneData.SpawnPoints;
            _assets = assets;
        }

        public GameObject CreatePlayer()
        {
            GameObject player;
            if (_spawnPoints.Count == 0)
            {
                player = CreatePlayerWithoutSpawnPoint();
            }
            else
            {
                player = CreatePlayerWithSpawnPoint(_spawnPoints[_spawnPointIndex].ToVectorWithOffset(),
                    Quaternion.identity);
                _spawnPointIndex = (_spawnPointIndex + 1) % _spawnPoints.Count;
            }

            return player;
        }

        public GameObject CreateSpectatorPlayer()
        {
            var spectator = _assets.Instantiate(SpectatorPlayerPath);
            spectator.GetComponent<Spectator>().Construct(_server);
            return spectator;
        }

        private GameObject CreatePlayerWithSpawnPoint(Vector3 position,
            Quaternion rotation) =>
            _assets.Instantiate(PlayerPath, position, rotation);

        private GameObject CreatePlayerWithoutSpawnPoint() => _assets.Instantiate(PlayerPath);
    }
}
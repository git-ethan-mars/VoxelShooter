using CameraLogic;
using Infrastructure.AssetManagement;
using Networking;
using UnityEngine;

namespace Infrastructure.Factory
{
    public class PlayerFactory : IPlayerFactory
    {
        private const string PlayerPath = "Prefabs/Player";
        private const string SpectatorPlayerPath = "Prefabs/Spectator player";

        private readonly IAssetProvider _assets;
        private readonly IServer _server;
        private readonly SpawnPointService _spawnPointService;

        public PlayerFactory(IServer server, IAssetProvider assets, SpawnPointService spawnPointService)
        {
            _server = server;
            _spawnPointService = spawnPointService;
            _assets = assets;
        }

        public GameObject CreatePlayer()
        {
            var player = _assets.Instantiate(PlayerPath, _spawnPointService.GetSpawnPosition(), Quaternion.identity);
            return player;
        }

        public GameObject CreateSpectatorPlayer()
        {
            var spectator = _assets.Instantiate(SpectatorPlayerPath);
            spectator.GetComponent<Spectator>().Construct(_server);
            return spectator;
        }
    }
}
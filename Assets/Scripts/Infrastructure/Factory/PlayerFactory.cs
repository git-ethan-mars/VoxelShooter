using System.Collections.Generic;
using CameraLogic;
using Data;
using Infrastructure.AssetManagement;
using Mirror;
using Networking;
using Networking.Synchronization;
using PlayerLogic;
using PlayerLogic.States;
using UnityEngine;

namespace Infrastructure.Factory
{
    public class PlayerFactory : IPlayerFactory
    {
        private readonly List<SpawnPoint> _spawnPoints;
        private readonly IAssetProvider _assets;
        private readonly IParticleFactory _particleFactory;
        private readonly IServer _server;
        private int _spawnPointIndex;
        private const string PlayerPath = "Prefabs/Player";
        private const string SpectatorPlayerPath = "Prefabs/Spectator player";


        public PlayerFactory(IAssetProvider assets,
            IServer server,
            IParticleFactory particleFactory)
        {
            _server = server;
            _spawnPoints = _server.MapProvider.MapData.SpawnPoints;
            _assets = assets;
            _particleFactory = particleFactory;
        }


        public void CreatePlayer(NetworkConnectionToClient connection)
        {
            GameObject player;
            if (_spawnPoints.Count == 0)
            {
                player = CreatePlayerWithoutSpawnPoint();
            }
            else
            {
                player = CreatePlayerWithSpawnPoint(_spawnPoints[_spawnPointIndex].ToUnityVector(),
                    Quaternion.identity);
                _spawnPointIndex = (_spawnPointIndex + 1) % _spawnPoints.Count;
            }

            var playerData = _server.ServerData.GetPlayerData(connection);
            playerData.PlayerStateMachine.Enter<LifeState>();
            ConfigurePlayer(player, playerData);
            ConfigureSynchronization(player);
            NetworkServer.AddPlayerForConnection(connection, player);
        }

        public void RespawnPlayer(NetworkConnectionToClient connection)
        {
            var result = _server.ServerData.TryGetPlayerData(connection, out var playerData);
            if (!result) return;
            GameObject player;
            if (_spawnPoints.Count == 0)
            {
                player = CreatePlayerWithoutSpawnPoint();
            }
            else
            {
                player = CreatePlayerWithSpawnPoint(_spawnPoints[_spawnPointIndex].ToUnityVector(),
                    Quaternion.identity);
                _spawnPointIndex = (_spawnPointIndex + 1) % _spawnPoints.Count;
            }

            playerData.PlayerStateMachine.Enter<LifeState>();
            ConfigurePlayer(player, playerData);
            ConfigureSynchronization(player);
            ReplacePlayer(connection, player);
        }

        public void CreateSpectatorPlayer(NetworkConnectionToClient connection)
        {
            var spectator = _assets.Instantiate(SpectatorPlayerPath);
            spectator.GetComponent<Spectator>().Construct(_server);
            ReplacePlayer(connection, spectator);
        }

        private GameObject CreatePlayerWithSpawnPoint(Vector3 position,
            Quaternion rotation) =>
            _assets.Instantiate(PlayerPath, position, rotation);

        private GameObject CreatePlayerWithoutSpawnPoint() => _assets.Instantiate(PlayerPath);

        private void ConfigurePlayer(GameObject player, PlayerData playerData)
        {
            player.GetComponent<Player>().Construct(playerData);
            player.GetComponent<PlayerMovement>().Construct(playerData.Characteristic);
            player.GetComponent<PlayerLogic.Inventory>().ItemIds
                .AddRange(playerData.ItemsId);
        }

        private void ConfigureSynchronization(GameObject player)
        {
            player.GetComponent<MapSynchronization>().Construct(_server.MapProvider);
            player.GetComponent<HealthSynchronization>().Construct(_server);
            player.GetComponent<RangeWeaponSynchronization>().Construct(_particleFactory, _assets, _server);
            player.GetComponent<MeleeWeaponSynchronization>().Construct(_particleFactory, _assets, _server);
        }

        private void ReplacePlayer(NetworkConnectionToClient connection, GameObject newPlayer)
        {
            if (connection.identity == null) return;
            var oldPlayer = connection.identity.gameObject;
            NetworkServer.ReplacePlayerForConnection(connection, newPlayer, true);
            Object.Destroy(oldPlayer, 0.1f);
        }
    }
}
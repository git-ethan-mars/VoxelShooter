using System.Collections.Generic;
using Data;
using Infrastructure.AssetManagement;
using Mirror;
using Networking;
using Networking.Synchronization;
using PlayerLogic;
using UnityEngine;

namespace Infrastructure.Factory
{
    public class PlayerFactory : IPlayerFactory
    {
        private readonly List<SpawnPoint> _spawnPoints;
        private int _spawnPointIndex;
        private readonly IAssetProvider _assets;
        private readonly ServerData _serverData;
        private readonly IParticleFactory _particleFactory;
        private const string PlayerPath = "Prefabs/Player";
        private const string SpectatorPlayerPath = "Prefabs/Spectator player";


        public PlayerFactory(IAssetProvider assets,
            ServerData serverData,
            IParticleFactory particleFactory)
        {
            _serverData = serverData;
            _spawnPoints = _serverData.Map.MapData.SpawnPoints;
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

            var playerData = _serverData.GetPlayerData(connection);
            ConfigurePlayer(player, playerData);
            ConfigureSynchronization(player);
            NetworkServer.AddPlayerForConnection(connection, player);
            
        }

        public void CreateSpectatorPlayer(NetworkConnectionToClient connection)
        {
            var spectator = _assets.Instantiate(SpectatorPlayerPath);
            ReplacePlayer(connection, spectator);
        }

        public void RespawnPlayer(NetworkConnectionToClient connection, PlayerData playerData)
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

            ConfigurePlayer(player, playerData);
            ConfigureSynchronization(player);
            ReplacePlayer(connection, player);
        }

        private GameObject CreatePlayerWithSpawnPoint(Vector3 position,
            Quaternion rotation) =>
            _assets.Instantiate(PlayerPath, position, rotation);

        private GameObject CreatePlayerWithoutSpawnPoint() => _assets.Instantiate(PlayerPath);

        private void ConfigurePlayer(GameObject player, PlayerData playerData)
        {
            player.GetComponent<Player>().Construct(playerData);
            player.GetComponent<PlayerMovement>().Construct(playerData.Characteristic);
            player.GetComponent<HealthSystem>().Construct(playerData.Characteristic);
            player.GetComponent<PlayerLogic.Inventory>().ItemIds
                .AddRange(playerData.ItemsId);
        }

        private void ConfigureSynchronization(GameObject player)
        {
            player.GetComponent<MapSynchronization>().Construct(_serverData.Map);
            player.GetComponent<HealthSynchronization>().Construct(_serverData, this);
            player.GetComponent<RangeWeaponSynchronization>().Construct(_particleFactory, _assets, _serverData);
            player.GetComponent<MeleeWeaponSynchronization>().Construct(_particleFactory, _serverData);
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
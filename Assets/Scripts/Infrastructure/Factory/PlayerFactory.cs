using System.Collections.Generic;
using System.Linq;
using Data;
using Infrastructure.AssetManagement;
using Infrastructure.Services;
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
        private readonly IStaticDataService _staticData;
        private readonly ServerData _serverData;
        private readonly IParticleFactory _particleFactory;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly ServerSettings _serverSettings;
        private const string PlayerPath = "Prefabs/Player";
        private const string SpectatorPlayerPath = "Prefabs/Spectator player";


        public PlayerFactory(ICoroutineRunner coroutineRunner, IAssetProvider assets, IStaticDataService staticData,
            ServerData serverData, ServerSettings serverSettings,
            IParticleFactory particleFactory)
        {
            _coroutineRunner = coroutineRunner;
            _serverData = serverData;
            _spawnPoints = _serverData.Map.MapData.SpawnPoints;
            _assets = assets;
            _staticData = staticData;
            _serverSettings = serverSettings;
            _particleFactory = particleFactory;
        }


        public void CreatePlayer(NetworkConnectionToClient connection, GameClass gameClass, string nickName)
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

            ConfigurePlayer(player, gameClass, nickName);
            ConfigureSynchronization(player);
            NetworkServer.AddPlayerForConnection(connection, player);
            _serverData.AddPlayer(connection, gameClass, nickName);
        }

        public void CreateSpectatorPlayer(NetworkConnectionToClient connection,
            GameClass gameClass)
        {
            _coroutineRunner.StartCoroutine(Utils.DoActionAfterDelay(_serverSettings.SpawnTime,
                () => RespawnPlayer(connection, gameClass,
                    _serverData.GetPlayerData(connection).NickName)));
            var spectator = _assets.Instantiate(SpectatorPlayerPath);
            ReplacePlayer(connection, spectator);
            _serverData.UpdatePlayerClass(connection, GameClass.None);
        }

        private void RespawnPlayer(NetworkConnectionToClient connection, GameClass gameClass, string nickName)
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

            ConfigurePlayer(player, gameClass, nickName);
            ConfigureSynchronization(player);
            ReplacePlayer(connection, player);
            _serverData.UpdatePlayerClass(connection, gameClass);
        }

        private GameObject CreatePlayerWithSpawnPoint(Vector3 position,
            Quaternion rotation) =>
            _assets.Instantiate(PlayerPath, position, rotation);

        private GameObject CreatePlayerWithoutSpawnPoint() => _assets.Instantiate(PlayerPath);

        private void ConfigurePlayer(GameObject player, GameClass gameClass, string nickName)
        {
            var characteristic = _staticData.GetPlayerCharacteristic(gameClass);
            player.GetComponent<Player>().Construct(characteristic, nickName);
            player.GetComponent<PlayerMovement>().Construct(characteristic);
            player.GetComponent<HealthSystem>().Construct(characteristic);
            player.GetComponent<PlayerLogic.Inventory>().ItemIds
                .AddRange(_staticData.GetInventory(gameClass).Select(item => item.id).ToList());
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
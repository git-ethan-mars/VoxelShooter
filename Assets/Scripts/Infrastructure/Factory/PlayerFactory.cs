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


        public GameObject CreatePlayer(GameClass gameClass, string nickName)
        {
            GameObject player;
            if (_spawnPoints.Count == 0)
            {
                player = CreatePlayerWithoutSpawnPoint(gameClass, nickName);
            }
            else
            {
                player = CreatePlayerWithSpawnPoint(gameClass, nickName, _spawnPoints[_spawnPointIndex].ToUnityVector(),
                    Quaternion.identity);
                _spawnPointIndex = (_spawnPointIndex + 1) % _spawnPoints.Count;
            }

            ConfigureSynchronization(player);
            return player;
        }

        public GameObject CreateSpectatorPlayer(NetworkConnectionToClient connection,
            GameClass gameClass)
        {
            _coroutineRunner.StartCoroutine(Utils.DoActionAfterDelay(_serverSettings.SpawnTime,
                () => RespawnPlayer(connection, gameClass)));
            return _assets.Instantiate(SpectatorPlayerPath);
        }

        private GameObject CreatePlayerWithSpawnPoint(GameClass gameClass, string nickName, Vector3 position,
            Quaternion rotation)
        {
            var player = _assets.Instantiate(PlayerPath, position, rotation);
            ConfigurePlayer(player, gameClass, nickName);
            return player;
        }

        private GameObject CreatePlayerWithoutSpawnPoint(GameClass gameClass, string nickName)
        {
            var player = _assets.Instantiate(PlayerPath);
            ConfigurePlayer(player, gameClass, nickName);
            return player;
        }

        private void ConfigurePlayer(GameObject player, GameClass gameClass, string nickName)
        {
            var characteristic = _staticData.GetPlayerCharacteristic(gameClass);
            player.GetComponent<Player>().Construct(characteristic, nickName);
            player.GetComponent<PlayerMovement>().Construct(characteristic);
            player.GetComponent<HealthSystem>().Construct(characteristic);
            var playerInventory = player.GetComponent<PlayerLogic.Inventory>();
            playerInventory.ItemIds.AddRange(_staticData.GetInventory(gameClass).Select(item => item.id).ToList());
        }

        private void ConfigureSynchronization(GameObject player)
        {
            player.GetComponent<MapSynchronization>().Construct(_serverData.Map);
            player.GetComponent<HealthSynchronization>().Construct(_serverData, this);
            player.GetComponent<RangeWeaponSynchronization>().Construct(_particleFactory, _assets, _serverData);
            player.GetComponent<MeleeWeaponSynchronization>().Construct(_particleFactory, _serverData);
        }

        private void RespawnPlayer(NetworkConnectionToClient connection, GameClass gameClass)
        {
            var player = CreatePlayer(gameClass,
                _serverData.GetPlayerData(connection).NickName);
            ConfigureSynchronization(player);
        }
    }
}
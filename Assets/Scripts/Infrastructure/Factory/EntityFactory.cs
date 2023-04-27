using System.Collections.Generic;
using System.Linq;
using Data;
using Infrastructure.AssetManagement;
using Infrastructure.Services;
using MapLogic;
using Mirror;
using Networking;
using Networking.Synchronization;
using PlayerLogic;
using UnityEngine;

namespace Infrastructure.Factory
{
    public class EntityFactory : IEntityFactory
    {
        private readonly List<SpawnPoint> _spawnPoints;
        private int _spawnPointIndex;
        private readonly IAssetProvider _assets;
        private readonly IStaticDataService _staticData;
        private readonly ServerData _serverData;
        private readonly IParticleFactory _particleFactory;
        private readonly Map _map;
        private const string PlayerPath = "Prefabs/Player";


        public EntityFactory(Map map, IAssetProvider assets, IStaticDataService staticData, ServerData serverData,
            IParticleFactory particleFactory)
        {
            _serverData = serverData;
            _map = map;
            _spawnPoints = map.MapData.SpawnPoints;
            _assets = assets;
            _staticData = staticData;
            _particleFactory = particleFactory;
        }

        public GameObject CreatePlayer(NetworkConnectionToClient connection, GameClass gameClass, string nickName)
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

        public GameObject RespawnPlayer(NetworkConnectionToClient connection, GameClass gameClass)
        {
            var player = CreatePlayer(connection, gameClass,
                _serverData.GetPlayerData(connection.connectionId).NickName);
            ConfigureSynchronization(player);
            return player;
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
            var movement = player.GetComponent<PlayerMovement>();
            movement.Construct(characteristic);
            player.GetComponent<HealthSystem>().Construct(characteristic);
            var inventory = _staticData.GetInventory(gameClass).Select(item => item.id).ToArray();
            var playerInventory = player.GetComponent<PlayerLogic.Inventory>();
            foreach (var itemId in inventory)
            {
                playerInventory.Ids.Add(itemId);
            }
        }

        private void ConfigureSynchronization(GameObject player)
        {
            player.GetComponent<MapSynchronization>().Construct(_map);
            player.GetComponent<HealthSynchronization>().Construct(_serverData, this);
            player.GetComponent<RangeWeaponSynchronization>().Construct(_particleFactory, _serverData);
            player.GetComponent<MeleeWeaponSynchronization>().Construct(_particleFactory, _serverData);
        }
    }
}
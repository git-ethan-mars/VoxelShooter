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
        private const string PlayerPath = "Prefabs/Player";


        public EntityFactory(Map map, IAssetProvider assets, IStaticDataService staticData, ServerData serverData,
            IParticleFactory particleFactory)
        {
            _serverData = serverData;
            _spawnPoints = map.MapData.SpawnPoints;
            _assets = assets;
            _staticData = staticData;
            _particleFactory = particleFactory;
        }

        public GameObject CreatePlayer(NetworkConnectionToClient conn, CharacterMessage message)
        {
            var gameClass = message.GameClass;
            var player = CreatePlayer(gameClass);
            ConfigureSynchronization(player);
            return player;

        }

        public GameObject RespawnPlayer(NetworkConnectionToClient connection, GameClass gameClass)
        {
            var player = CreatePlayer(gameClass);
            ConfigureSynchronization(player);
            return player;
        }

        private GameObject CreatePlayer(GameClass gameClass)
        {
            GameObject player;
            if (_spawnPoints.Count == 0)
            {
                player = CreatePlayerWithoutSpawnPoint(gameClass);
            }
            else
            {
                player = CreatePlayerWithSpawnPoint(gameClass, _spawnPoints[_spawnPointIndex].ToUnityVector(),
                    Quaternion.identity);
                _spawnPointIndex = (_spawnPointIndex + 1) % _spawnPoints.Count;
            }

            return player;
        }

        private GameObject CreatePlayerWithSpawnPoint(GameClass gameClass, Vector3 position, Quaternion rotation)
        {
            var player = _assets.Instantiate(PlayerPath, position, rotation);
            ConfigurePlayer(player, gameClass);
            return player;
        }

        private GameObject CreatePlayerWithoutSpawnPoint(GameClass gameClass)
        {
            var player = _assets.Instantiate(PlayerPath);
            ConfigurePlayer(player, gameClass);
            return player;
        }

        private void ConfigurePlayer(GameObject player, GameClass gameClass)
        {
            var characteristic = _staticData.GetPlayerCharacteristic(gameClass);
            player.GetComponent<Player>().Construct(characteristic);
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
            player.GetComponent<HealthSynchronization>().Construct(_serverData, this);
            player.GetComponent<WeaponSynchronization>().Construct(_particleFactory, _serverData);
        }
    }
}
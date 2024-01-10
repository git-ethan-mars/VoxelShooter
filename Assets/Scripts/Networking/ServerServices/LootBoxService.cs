using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using Entities;
using Infrastructure;
using Infrastructure.Factory;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Networking.ServerServices
{
    public class BoxDropService
    {
        private const string LootBoxContainer = "LootBoxContainer";

        private readonly ICoroutineRunner _coroutineRunner;
        private readonly IEntityFactory _entityFactory;
        private readonly IServer _server;
        private readonly int _spawnRate;
        private readonly Transform _parent;
        private IEnumerator _coroutine;
        private readonly List<Vector3Int> _lootBoxSpawnPositions;

        public BoxDropService(IServer server, CustomNetworkManager networkManager, ServerSettings serverSettings)
        {
            _coroutineRunner = networkManager;
            _spawnRate = serverSettings.BoxSpawnTime;
            _entityFactory = networkManager.EntityFactory;
            _server = server;
            _parent = networkManager.GameFactory.CreateGameObjectContainer(LootBoxContainer);
            _lootBoxSpawnPositions = GetLootBoxSpawnPositions();
        }

        public void Start()
        {
            _coroutine = SpawnLootBox();
            _coroutineRunner.StartCoroutine(_coroutine);
        }

        public void Stop()
        {
            _coroutineRunner.StopCoroutine(_coroutine);
        }

        private List<Vector3Int> GetLootBoxSpawnPositions()
        {
            var spawnPositions = new List<Vector3Int>();
            for (var x = 0; x < _server.MapProvider.Width; x++)
            {
                for (var z = 0; z < _server.MapProvider.Depth; z++)
                {
                    if (_server.MapProvider.GetBlockByGlobalPosition(x, 1, z).IsSolid())
                    {
                        spawnPositions.Add(new Vector3Int(x, _server.MapProvider.Height - 1, z));
                    }
                }
            }

            return spawnPositions;
        }

        // If build up top layer, game will go into infinite loop
        private IEnumerator SpawnLootBox()
        {
            while (true)
            {
                var spawnPosition = _lootBoxSpawnPositions[Random.Range(0, _lootBoxSpawnPositions.Count - 1)];
                var spawnBlock = _server.MapProvider.GetBlockByGlobalPosition(spawnPosition.x,
                    spawnPosition.y, spawnPosition.z);
                if (spawnBlock.IsSolid())
                    continue;
                var spawnCoordinates = spawnPosition + Constants.worldOffset;
                LootBox lootBox;
                var lootBoxType = (LootBoxType) Random.Range(0, Enum.GetNames(typeof(LootBoxType)).Length);
                switch (lootBoxType)
                {
                    case LootBoxType.Ammo:
                        lootBox = _entityFactory.CreateAmmoBox(spawnCoordinates, _parent);
                        break;
                    case LootBoxType.Health:
                        lootBox = _entityFactory.CreateHealthBox(spawnCoordinates, _parent);
                        break;
                    default:
                        lootBox = _entityFactory.CreateBlockBox(spawnCoordinates, _parent);
                        break;
                }

                lootBox.Construct(_server);
                _server.EntityContainer.AddLootBox(lootBox);
                _server.EntityContainer.AddPushable(lootBox);
                NetworkServer.Spawn(lootBox.gameObject);
                lootBox.PickedUp += OnPickedUp;
                yield return new WaitForSeconds(_spawnRate);
            }
        }

        private void OnPickedUp(LootBox lootBox, NetworkConnectionToClient connection)
        {
            _server.EntityContainer.RemoveLootBox(lootBox);
            _server.EntityContainer.RemovePushable(lootBox);
            lootBox.PickedUp -= OnPickedUp;
            NetworkServer.Destroy(lootBox.gameObject);
        }
    }
}
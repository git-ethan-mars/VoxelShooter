using System.Collections.Generic;
using Entities;
using Infrastructure.AssetManagement;
using Infrastructure.Factory;
using Mirror;
using UnityEngine;

namespace Networking.ClientServices
{
    public class ClientPrefabRegistrar
    {
        private const string LootBoxContainer = "LootBoxContainer";

        public HashSet<LootBox> LootBoxes
        {
            get => _lootBoxes;
            set => _lootBoxes = value;
        }

        private readonly IAssetProvider _assets;
        private readonly IPlayerFactory _playerFactory;
        private readonly IEntityFactory _entityFactory;
        private readonly Transform _lootBoxContainer;
        private HashSet<LootBox> _lootBoxes;

        public ClientPrefabRegistrar(IAssetProvider assets, IGameFactory gameFactory,
            IPlayerFactory playerFactory,
            IEntityFactory entityFactory)
        {
            _assets = assets;
            _playerFactory = playerFactory;
            _entityFactory = entityFactory;
            _lootBoxes = new HashSet<LootBox>();
            if (!NetworkClient.activeHost)
            {
                _lootBoxContainer = gameFactory.CreateGameObjectContainer(LootBoxContainer);
            }
        }

        public void RegisterPrefabs()
        {
            NetworkClient.RegisterPrefab(_assets.Load<GameObject>(PlayerPath.MainPlayerPath), SpawnPlayerHandler,
                UnSpawnPlayerHandler);
            NetworkClient.RegisterPrefab(_assets.Load<GameObject>(EntityPath.HealthBoxPath), SpawnHealthBox,
                UnSpawnLootBox);
            NetworkClient.RegisterPrefab(_assets.Load<GameObject>(EntityPath.AmmoBoxPath), SpawnAmmoBox,
                UnSpawnLootBox);
            NetworkClient.RegisterPrefab(_assets.Load<GameObject>(EntityPath.BlockBoxPath), SpawnBlockBox,
                UnSpawnLootBox);
        }

        public void UnregisterPrefabs()
        {
            NetworkClient.UnregisterPrefab(_assets.Load<GameObject>(PlayerPath.MainPlayerPath));
            NetworkClient.UnregisterPrefab(_assets.Load<GameObject>(EntityPath.HealthBoxPath));
            NetworkClient.UnregisterPrefab(_assets.Load<GameObject>(EntityPath.AmmoBoxPath));
            NetworkClient.UnregisterPrefab(_assets.Load<GameObject>(EntityPath.BlockBoxPath));
        }

        private GameObject SpawnPlayerHandler(SpawnMessage message)
        {
            return _playerFactory.CreatePlayer(message.position);
        }

        private void UnSpawnPlayerHandler(GameObject spawned)
        {
            Object.Destroy(spawned);
        }

        private GameObject SpawnHealthBox(SpawnMessage message)
        {
            var lootBox = _entityFactory.CreateHealthBox(message.position, _lootBoxContainer);
            _lootBoxes.Add(lootBox);
            return lootBox.gameObject;
        }

        private GameObject SpawnAmmoBox(SpawnMessage message)
        {
            var lootBox = _entityFactory.CreateAmmoBox(message.position, _lootBoxContainer);
            _lootBoxes.Add(lootBox);
            return lootBox.gameObject;
        }

        private GameObject SpawnBlockBox(SpawnMessage message)
        {
            var lootBox = _entityFactory.CreateBlockBox(message.position, _lootBoxContainer);
            _lootBoxes.Add(lootBox);
            return lootBox.gameObject;
        }

        private void UnSpawnLootBox(GameObject spawned)
        {
            _lootBoxes.Remove(spawned.GetComponent<LootBox>());
            Object.Destroy(spawned);
        }
    }
}
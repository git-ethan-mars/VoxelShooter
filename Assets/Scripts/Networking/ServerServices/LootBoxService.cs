using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using Entities;
using Infrastructure;
using Infrastructure.Factory;
using Mirror;
using Networking.Messages.Responses;
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
        private readonly Dictionary<LootBox, LootBoxType> _lootBoxes = new();
        private readonly EntityPositionValidator _entityPositionValidator;
        private readonly Transform _parent;
        private IEnumerator _coroutine;
        private readonly List<Vector3Int> _lootBoxSpawnPositions;

        public BoxDropService(IServer server, CustomNetworkManager networkManager, ServerSettings serverSettings,
            EntityPositionValidator entityPositionValidator)
        {
            _coroutineRunner = networkManager;
            _spawnRate = serverSettings.BoxSpawnTime;
            _entityFactory = networkManager.EntityFactory;
            _server = server;
            _entityPositionValidator = entityPositionValidator;
            _parent = networkManager.GameFactory.CreateGameObjectContainer(LootBoxContainer).transform;
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
            for (var x = 0; x < _server.MapProvider.MapData.Width; x++)
            {
                for (var z = 0; z < _server.MapProvider.MapData.Depth; z++)
                {
                    if (_server.MapProvider.GetBlockByGlobalPosition(x, 1, z).IsSolid())
                    {
                        spawnPositions.Add(new Vector3Int(x, _server.MapProvider.MapData.Height - 1, z));
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

                NetworkServer.Spawn(lootBox.gameObject);
                _entityPositionValidator.AddEntity(lootBox);
                _lootBoxes.Add(lootBox, lootBoxType);
                lootBox.OnPickUp += OnPickUp;

                yield return new WaitForSeconds(_spawnRate);
            }
        }

        private void OnPickUp(LootBox lootBox, NetworkConnectionToClient connection)
        {
            _entityPositionValidator.RemoveEntity(lootBox);
            if (_lootBoxes[lootBox] == LootBoxType.Ammo)
            {
                PickUpAmmoBox(connection);
            }

            if (_lootBoxes[lootBox] == LootBoxType.Health)
            {
                PickUpHealthBox(connection);
            }

            if (_lootBoxes[lootBox] == LootBoxType.Block)
            {
                PickUpBlockBox(connection);
            }

            lootBox.OnPickUp -= OnPickUp;
            NetworkServer.Destroy(lootBox.gameObject);
        }

        private void PickUpHealthBox(NetworkConnectionToClient connection)
        {
            _server.Heal(connection, 50);
        }

        private void PickUpBlockBox(NetworkConnectionToClient receiver)
        {
            var result = _server.Data.TryGetPlayerData(receiver, out var playerData);
            if (!result || !playerData.IsAlive)
            {
                return;
            }

            for (var i = 0; i < playerData.Items.Count; i++)
            {
                if (playerData.Items[i].itemType == ItemType.Block)
                {
                    var blockItemData = (BlockItemData) playerData.ItemData[i];
                    blockItemData.Amount += 50;
                    receiver.Send(new ItemUseResponse(i, blockItemData.Amount));
                }
            }
        }

        private void PickUpAmmoBox(NetworkConnectionToClient receiver)
        {
            var result = _server.Data.TryGetPlayerData(receiver, out var playerData);
            if (!result || !playerData.IsAlive) return;

            for (var i = 0; i < playerData.Items.Count; i++)
            {
                var item = playerData.Items[i];
                var itemData = playerData.ItemData[i];
                if (item.itemType == ItemType.RangeWeapon)
                {
                    var rangeWeapon = (RangeWeaponItem) item;
                    var rangeWeaponData = (RangeWeaponItemData) itemData;
                    rangeWeaponData.TotalBullets += rangeWeapon.magazineSize * 2;
                    receiver.Send(new ReloadResultResponse(i, rangeWeaponData.TotalBullets,
                        rangeWeaponData.BulletsInMagazine));
                    continue;
                }

                if (item.itemType == ItemType.Tnt)
                {
                    var tntData = (TntItemData) itemData;
                    tntData.Amount += 1;
                    receiver.Send(new ItemUseResponse(i, tntData.Amount));
                    continue;
                }

                if (item.itemType == ItemType.Grenade)
                {
                    var grenadeData = (GrenadeItemData) itemData;
                    grenadeData.Amount += 1;
                    receiver.Send(new ItemUseResponse(i, grenadeData.Amount));
                    continue;
                }

                if (item.itemType == ItemType.RocketLauncher)
                {
                    var rocketLauncherData = (RocketLauncherItemData) itemData;
                    rocketLauncherData.CarriedRockets += 1;
                    receiver.Send(new RocketReloadResponse(i, rocketLauncherData.ChargedRockets,
                        rocketLauncherData.CarriedRockets));
                }
            }
        }
    }
}
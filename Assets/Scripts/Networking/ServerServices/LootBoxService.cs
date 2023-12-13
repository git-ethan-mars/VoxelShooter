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

        public BoxDropService(IServer server, CustomNetworkManager networkManager, ServerSettings serverSettings,
            EntityPositionValidator entityPositionValidator)
        {
            _coroutineRunner = networkManager;
            _spawnRate = serverSettings.BoxSpawnTime;
            _entityFactory = networkManager.EntityFactory;
            _server = server;
            _entityPositionValidator = entityPositionValidator;
            _parent = networkManager.GameFactory.CreateGameObjectContainer(LootBoxContainer).transform;
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

        // If build up top layer, game will go into infinite loop
        private IEnumerator SpawnLootBox()
        {
            while (true)
            {
                var boxSpawnLayer = _server.MapProvider.MapData.BoxSpawnLayer;
                var spawnPosition = boxSpawnLayer[Random.Range(0, boxSpawnLayer.Count - 1)];
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
                    playerData.CountByItem[playerData.Items[i]] += 50;
                    receiver.Send(new ItemUseResponse(i, playerData.CountByItem[playerData.Items[i]]));
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
                    var rangeWeaponData = (RangeWeaponData) itemData;
                    rangeWeaponData.TotalBullets += rangeWeapon.magazineSize * 2;
                    receiver.Send(new ReloadResultResponse(i, rangeWeaponData.TotalBullets,
                        rangeWeaponData.BulletsInMagazine));
                    continue;
                }

                if (item.itemType == ItemType.Tnt)
                {
                    playerData.CountByItem[item] += 1;
                    receiver.Send(new ItemUseResponse(i, playerData.CountByItem[item]));
                    continue;
                }

                if (item.itemType == ItemType.Grenade)
                {
                    playerData.CountByItem[item] += 1;
                    receiver.Send(new ItemUseResponse(i, playerData.CountByItem[item]));
                    continue;
                }

                if (item.itemType == ItemType.RocketLauncher)
                {
                    playerData.CountByItem[item] += 1;
                    receiver.Send(new ItemUseResponse(i, playerData.CountByItem[item]));
                }
            }
        }
    }
}
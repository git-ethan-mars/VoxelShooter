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
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly int _timeInSeconds;
        private IEntityFactory _entityFactory;
        private readonly IServer _server;
        private const int SpawnRate = 10;
        private Dictionary<LootBox, LootBoxType> _lootBoxes = new();
        private readonly EntityPositionValidator _entityPostionValidator;
        private readonly IGameFactory _gameFactory;
        private readonly Transform _parent;
        private const string LootBoxContainer = "LootBoxContainer";

        public BoxDropService(IServer server, ICoroutineRunner coroutineRunner, int timeInMinutes,
            IEntityFactory entityFactory, EntityPositionValidator entityPositionValidator, IGameFactory gameFactory)
        {
            _coroutineRunner = coroutineRunner;
            _timeInSeconds = timeInMinutes * 60;
            _entityFactory = entityFactory;
            _server = server;
            _entityPostionValidator = entityPositionValidator;
            _gameFactory = gameFactory;
            _parent = _gameFactory.CreateGameObjectContainer(LootBoxContainer).transform;
        }

        public void Start()
        {
            _coroutineRunner.StartCoroutine(SpawnLootBox());
        }
        
        private IEnumerator SpawnLootBox()
        {
            for (var i = 0; i < _timeInSeconds; i++)
            {
                if (i % SpawnRate == 0)
                {
                    var lowerSolidLayer = _server.MapProvider.MapData.LowerSolidLayer;
                    var spawnPosition = lowerSolidLayer[Random.Range(0, lowerSolidLayer.Count - 1)] + Constants.worldOffset;
                    var mapHeight = _server.MapProvider.MapData.Height + 1;
                    LootBox lootBox;
                    var lootBoxType = (LootBoxType)Random.Range(0, Enum.GetNames(typeof(LootBoxType)).Length);
                    switch (lootBoxType)
                    {
                        case LootBoxType.Ammo:
                            lootBox = _entityFactory.CreateAmmoBox(new Vector3(spawnPosition.x, mapHeight,
                                spawnPosition.z), _parent);
                            break;
                        case LootBoxType.Health:
                            lootBox = _entityFactory.CreateHealthBox(new Vector3(spawnPosition.x, mapHeight,
                                spawnPosition.z), _parent);
                            break;
                        default:
                            lootBox = _entityFactory.CreateBlockBox(new Vector3(spawnPosition.x, mapHeight,
                                spawnPosition.z), _parent);
                            break;
                    }
                    _entityPostionValidator.AddEntity(lootBox);
                    _lootBoxes.Add(lootBox, lootBoxType);
                    lootBox.OnPickUp += OnPickUp;
                }
                yield return new WaitForSeconds(1);
            }
        }

        private void OnPickUp(LootBox lootBox, NetworkConnectionToClient connection)
        {
            _entityPostionValidator.RemoveEntity(lootBox);
            if (_lootBoxes[lootBox] == LootBoxType.Ammo)
            {
                PickUpAmmo(connection);
            }
            
            if (_lootBoxes[lootBox] == LootBoxType.Health)
            {
                PickUpHealthBox(connection);
            }
            
            if (_lootBoxes[lootBox] == LootBoxType.Block)
            {
                PickUpBox(connection);
            }

            lootBox.OnPickUp -= OnPickUp;
            NetworkServer.Destroy(lootBox.gameObject);
        }

        private void PickUpHealthBox(NetworkConnectionToClient connection)
        {
            _server.Heal(connection, 50);
        }

        private void PickUpBox(NetworkConnectionToClient receiver)
        {
            var result = _server.Data.TryGetPlayerData(receiver, out var playerData);
            if (!result || !playerData.IsAlive) return;
            
            foreach (var itemId in playerData.ItemIds)
            {
                var item = _server.Data.StaticData.GetItem(itemId);

                if (item.itemType == ItemType.Block)
                {
                    playerData.ItemCountById[itemId] += 50;
                    receiver.Send(new ItemUseResponse(itemId, playerData.ItemCountById[itemId]));
                }
            }
        }
        
        private void PickUpAmmo(NetworkConnectionToClient receiver)
        {
            var result = _server.Data.TryGetPlayerData(receiver, out var playerData);
            if (!result || !playerData.IsAlive) return;
            
            foreach (var itemId in playerData.ItemIds)
            {
                var item = _server.Data.StaticData.GetItem(itemId);
                if (item.itemType == ItemType.RangeWeapon)
                {
                    var weapon = playerData.RangeWeaponsById[itemId];
                    weapon.TotalBullets += weapon.MagazineSize * 2;
                    receiver.Send(new ReloadResultResponse(itemId, weapon.TotalBullets, weapon.BulletsInMagazine));
                    continue;
                }

                if (item.itemType == ItemType.Tnt)
                {
                    playerData.ItemCountById[itemId] += 1;
                    receiver.Send(new ItemUseResponse(itemId, playerData.ItemCountById[itemId]));
                    continue;
                }
                
                if (item.itemType == ItemType.Grenade)
                {
                    playerData.ItemCountById[itemId] += 1;
                    receiver.Send(new ItemUseResponse(itemId, playerData.ItemCountById[itemId]));
                    continue;
                }

                if (item.itemType == ItemType.RocketLauncher)
                {
                    playerData.ItemCountById[itemId] += 1;
                    receiver.Send(new ItemUseResponse(itemId, playerData.ItemCountById[itemId]));
                }
            }
        }
    }
}
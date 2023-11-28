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
        private readonly int _timeInSeconds;
        private readonly IEntityFactory _entityFactory;
        private readonly IServer _server;
        private readonly int _spawnRate;
        private readonly Dictionary<LootBox, LootBoxType> _lootBoxes = new();
        private readonly EntityPositionValidator _entityPositionValidator;
        private readonly Transform _parent;

        public BoxDropService(IServer server, ICoroutineRunner coroutineRunner, ServerSettings serverSettings,
            IEntityFactory entityFactory, EntityPositionValidator entityPositionValidator, IGameFactory gameFactory)
        {
            _coroutineRunner = coroutineRunner;
            _timeInSeconds = serverSettings.MaxDuration * 60;
            _spawnRate = serverSettings.BoxSpawnTime;
            _entityFactory = entityFactory;
            _server = server;
            _entityPositionValidator = entityPositionValidator;
            _parent = gameFactory.CreateGameObjectContainer(LootBoxContainer).transform;
        }

        public void Start()
        {
            _coroutineRunner.StartCoroutine(SpawnLootBox());
        }

        public void Stop()
        {
            _coroutineRunner.StopCoroutine(SpawnLootBox());
        }

        // If build up top layer, game will go into infinite loop
        private IEnumerator SpawnLootBox()
        {
            for (var i = 0; i < _timeInSeconds; i++)
            {
                if (i % _spawnRate == 0)
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
                }

                yield return new WaitForSeconds(1);
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

            for (var i = 0; i < playerData.ItemIds.Count; i++)
            {
                var item = _server.Data.StaticData.GetItem(playerData.ItemIds[i]);
                if (item.itemType == ItemType.Block)
                {
                    playerData.ItemCountById[playerData.ItemIds[i]] += 50;
                    receiver.Send(new ItemUseResponse(i, playerData.ItemCountById[playerData.ItemIds[i]]));
                }
            }
        }

        private void PickUpAmmoBox(NetworkConnectionToClient receiver)
        {
            var result = _server.Data.TryGetPlayerData(receiver, out var playerData);
            if (!result || !playerData.IsAlive) return;

            for (var i = 0; i < playerData.ItemIds.Count; i++)
            {
                var item = _server.Data.StaticData.GetItem(playerData.ItemIds[i]);
                if (item.itemType == ItemType.RangeWeapon)
                {
                    var weapon = playerData.RangeWeaponsById[playerData.ItemIds[i]];
                    weapon.TotalBullets += weapon.MagazineSize * 2;
                    receiver.Send(new ReloadResultResponse(i, weapon.TotalBullets, weapon.BulletsInMagazine));
                    continue;
                }

                if (item.itemType == ItemType.Tnt)
                {
                    playerData.ItemCountById[playerData.ItemIds[i]] += 1;
                    receiver.Send(new ItemUseResponse(i, playerData.ItemCountById[playerData.ItemIds[i]]));
                    continue;
                }

                if (item.itemType == ItemType.Grenade)
                {
                    playerData.ItemCountById[playerData.ItemIds[i]] += 1;
                    receiver.Send(new ItemUseResponse(i, playerData.ItemCountById[playerData.ItemIds[i]]));
                    continue;
                }

                if (item.itemType == ItemType.RocketLauncher)
                {
                    playerData.ItemCountById[playerData.ItemIds[i]] += 1;
                    receiver.Send(new ItemUseResponse(i, playerData.ItemCountById[playerData.ItemIds[i]]));
                }
            }
        }
    }
}
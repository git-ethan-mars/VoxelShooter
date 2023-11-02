using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
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
        private const int SpawnRate = 5;
        private Dictionary<LootBox, LootBoxType> _lootBoxes = new Dictionary<LootBox, LootBoxType>();

        public BoxDropService(IServer server, ICoroutineRunner coroutineRunner, int timeInMinutes,
            IEntityFactory entityFactory)
        {
            _coroutineRunner = coroutineRunner;
            _timeInSeconds = timeInMinutes * 60;
            _entityFactory = entityFactory;
            _server = server;
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
                    var lootBox = _entityFactory.CreateLootBox(new Vector3(100, 100, 100));
                    var lootBoxType = Random.Range(0, 2);
                    _lootBoxes.Add(lootBox, (LootBoxType)lootBoxType);
                    lootBox.OnPickUp += OnPickUp;
                }
                yield return new WaitForSeconds(1);
            }
        }

        private void OnPickUp(LootBox lootBox, NetworkConnectionToClient connection)
        {
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
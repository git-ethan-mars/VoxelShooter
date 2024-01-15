using System.Collections.Generic;
using System.Linq;
using CameraLogic;
using Data;
using Infrastructure.Factory;
using Infrastructure.Services.Input;
using Infrastructure.Services.StaticData;
using Inventory.Block;
using Inventory.Drill;
using Inventory.Grenade;
using Inventory.MeleeWeapon;
using Inventory.RangeWeapon;
using Inventory.RocketLauncher;
using Inventory.Tnt;
using Mirror;
using Networking.MessageHandlers.ResponseHandler;
using Networking.Messages.Requests;
using PlayerLogic;
using UI;
using UnityEngine;

namespace Inventory
{
    public class InventorySystem
    {
        private readonly List<IInventoryItemState> _states = new();
        private readonly ChangeSlotResultHandler _changeSlotHandler;
        private readonly ItemUseHandler _itemUseHandler;
        private readonly ReloadResultHandler _reloadHandler;
        private readonly ShootResultHandler _shootHandler;
        private readonly RocketSpawnHandler _rocketSpawnHandler;
        private readonly RocketReloadHandler _rocketReloadHandler;
        private readonly DrillReloadHandler _drillReloadHandler;
        private readonly DrillSpawnHandler _drillSpawnHandler;
        private readonly InventoryInput _inventoryInput;
        private readonly Hud _hud;
        private int _index;
        
        public InventorySystem(IInputService inputService, IStaticDataService staticData,
            IMeshFactory meshFactory, List<int> itemIds, Hud hud, Player player)
        {
            var rayCaster = new RayCaster(Camera.main);
            _inventoryInput = new InventoryInput(inputService);
            _inventoryInput.SlotButtonPressed += ChangeSlotRequest;
            _inventoryInput.MouseScrolledUp += SendIncrementSlotIndexRequest;
            _inventoryInput.MouseScrolledDown += SendDecrementSlotIndexRequest;
            _hud = hud;
            foreach (var item in itemIds.Select(staticData.GetItem))
            {
                if (item.itemType == ItemType.RangeWeapon)
                {
                    var rangeWeapon = (RangeWeaponItem) item;
                    _states.Add(new RangeWeaponState(_inventoryInput, rayCaster,
                        rangeWeapon, new RangeWeaponItemData(rangeWeapon), player, hud));
                }

                if (item.itemType == ItemType.MeleeWeapon)
                {
                    _states.Add(new MeleeWeaponState(_inventoryInput, rayCaster, player,
                        (MeleeWeaponItem) item));
                }

                if (item.itemType == ItemType.Block)
                {
                    var block = (BlockItem) item;
                    _states.Add(new BlockState(meshFactory, _inventoryInput, block, new BlockItemData(block), rayCaster,
                        player, hud));
                }

                if (item.itemType == ItemType.Grenade)
                {
                    var grenade = (GrenadeItem) item;
                    _states.Add(
                        new GrenadeState(_inventoryInput, rayCaster, grenade, new GrenadeItemData(grenade), hud));
                }

                if (item.itemType == ItemType.Tnt)
                {
                    var tnt = (TntItem) item;
                    _states.Add(new TntState(meshFactory, _inventoryInput, rayCaster, tnt, new TntItemData(tnt), player,
                        hud));
                }

                if (item.itemType == ItemType.RocketLauncher)
                {
                    var rocketLauncher = (RocketLauncherItem) item;
                    _states.Add(new RocketLauncherState(_inventoryInput, rayCaster,
                        (RocketLauncherItem) item, hud, new RocketLauncherItemData(rocketLauncher)));
                }
                
                if (item.itemType == ItemType.Drill)
                {
                    var drill = (DrillItem) item;
                    _states.Add(new DrillState(_inventoryInput, rayCaster,
                        (DrillItem) item, hud, new DrillItemData(drill)));
                }
            }

            for (var i = 0; i < _states.Count; i++)
            {
                var previousColor = _hud.Slots[i].color;
                _hud.Slots[i].color = new Color(previousColor.r, previousColor.g, previousColor.b, 255);
                _hud.Slots[i].sprite = _states[i].ItemView.Icon;
            }

            _changeSlotHandler = new ChangeSlotResultHandler(this);
            _changeSlotHandler.Register();
            _itemUseHandler = new ItemUseHandler(this);
            _itemUseHandler.Register();
            _reloadHandler = new ReloadResultHandler(this);
            _reloadHandler.Register();
            _shootHandler = new ShootResultHandler(this);
            _shootHandler.Register();
            _rocketSpawnHandler = new RocketSpawnHandler(this);
            _rocketSpawnHandler.Register();
            _rocketReloadHandler = new RocketReloadHandler(this);
            _rocketReloadHandler.Register();
            _drillSpawnHandler = new DrillSpawnHandler(this);
            _drillSpawnHandler.Register();
            _drillReloadHandler = new DrillReloadHandler(this);
            _drillReloadHandler.Register();

            ChangeSlotRequest(_index);
        }

        public void Update()
        {
            _states[_index].Update();
            _inventoryInput.Update();
        }

        public void OnDestroy()
        {
            _inventoryInput.SlotButtonPressed -= ChangeSlotRequest;
            _inventoryInput.MouseScrolledUp -= SendIncrementSlotIndexRequest;
            _inventoryInput.MouseScrolledDown -= SendDecrementSlotIndexRequest;
            _states[_index].Exit();
            foreach (var state in _states)
            {
                state.Dispose();
            }

            _changeSlotHandler.Unregister();
            _itemUseHandler.Unregister();
            _reloadHandler.Unregister();
            _shootHandler.Unregister();
            _rocketReloadHandler.Unregister();
            _rocketSpawnHandler.Unregister();
            _drillReloadHandler.Unregister();
            _drillSpawnHandler.Unregister();
        }

        public void SwitchActiveSlot(int slotIndex)
        {
            _states[_index].Exit();
            _hud.Boarders[_index].SetActive(false);
            _index = slotIndex;
            _states[_index].Enter();
            _hud.Boarders[_index].SetActive(true);
        }

        public IInventoryItemModel GetModel(int index)
        {
            return _states[index].ItemModel;
        }

        private void ChangeSlotRequest(int slotIndex)
        {
            NetworkClient.Send(new ChangeSlotRequest(slotIndex));
        }

        private void SendIncrementSlotIndexRequest()
        {
            NetworkClient.Send(new IncrementSlotIndexRequest());
        }

        private void SendDecrementSlotIndexRequest()
        {
            NetworkClient.Send(new DecrementSlotIndexRequest());
        }
    }
}
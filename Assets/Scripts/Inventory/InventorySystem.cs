using System.Collections.Generic;
using System.Linq;
using CameraLogic;
using Data;
using Infrastructure.Factory;
using Infrastructure.Services.Input;
using Infrastructure.Services.StaticData;
using Inventory.Block;
using Inventory.Grenade;
using Inventory.MeleeWeapon;
using Inventory.RangeWeapon;
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
        public IInventoryItemModel ActiveItemModel => _states[_index].ItemModel;
        private int _index;
        private readonly List<IInventoryItemState> _states = new();
        private readonly ChangeSlotResultHandler _changeSlotHandler;
        private readonly ItemUseHandler _itemUseHandler;
        private readonly ReloadResultHandler _reloadHandler;
        private readonly ShootResultHandler _shootHandler;
        private readonly InventoryInput _inventoryInput;
        private readonly Hud _hud;

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
                    _states.Add(new RangeWeaponState(_inventoryInput, rayCaster,
                        new RangeWeaponData((RangeWeaponItem) item), Camera.main, hud));
                }

                if (item.itemType == ItemType.MeleeWeapon)
                {
                    _states.Add(new MeleeWeaponState(_inventoryInput, rayCaster, player,
                        new MeleeWeaponData((MeleeWeaponItem) item)));
                }

                if (item.itemType == ItemType.Block)
                {
                    _states.Add(new BlockState(meshFactory, _inventoryInput, (BlockItem) item, rayCaster,
                        player, hud));
                }

                if (item.itemType == ItemType.Grenade)
                {
                    _states.Add(new GrenadeState(_inventoryInput, rayCaster, (GrenadeItem) item, hud));
                }

                if (item.itemType == ItemType.Tnt)
                {
                    _states.Add(new TntState(meshFactory, _inventoryInput, rayCaster, (TntItem) item,
                        player, hud));
                }

                if (item.itemType == ItemType.RocketLauncher)
                {
                    _states.Add(new RocketLauncherState(_inventoryInput, rayCaster,
                        (RocketLauncherItem) item, hud));
                }
            }

            _states[_index].Enter();

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
        }

        public void Update()
        {
            _states[_index].Update();
            _inventoryInput.Update();
        }

        public void Clear()
        {
            _inventoryInput.SlotButtonPressed -= ChangeSlotRequest;
            _inventoryInput.MouseScrolledUp -= SendIncrementSlotIndexRequest;
            _inventoryInput.MouseScrolledDown -= SendDecrementSlotIndexRequest;
            _states[_index].Exit();
            _changeSlotHandler.Unregister();
            _itemUseHandler.Unregister();
            _reloadHandler.Unregister();
            _shootHandler.Unregister();
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
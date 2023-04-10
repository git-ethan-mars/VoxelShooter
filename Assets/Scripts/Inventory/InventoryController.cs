using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Infrastructure;
using Infrastructure.AssetManagement;
using Infrastructure.Factory;
using Infrastructure.Services.Input;
using Networking.Synchronization;
using Rendering;
using UI;
using UnityEngine;

namespace Inventory
{
    public class InventoryController : MonoBehaviour, ICoroutineRunner
    {
        [SerializeField] private float placeDistance;
        [SerializeField] private Transform itemPosition;
        [SerializeField] private InventoryView inventoryView;
        private IInputService _inputService;
        private List<IInventoryItemView> _inventory;
        private int _itemIndex;
        private int _maxIndex;
        private GameObject[] _boarders;
        private CubeRenderer _cubeRenderer;
        private Camera _mainCamera;
        private MapSynchronization _mapSynchronization;
        private GameObject _palette;
        private IAssetProvider _assets;
        private IGameFactory _gameFactory;
        private GameObject _player;
        private GameObject _hud;

        public void Construct(IAssetProvider assets, IGameFactory gameFactory, GameObject hud, GameObject player)
        {
            AddEventHandlers(player.GetComponent<InventoryInput>());
            _player = player;
            _hud = hud;
            _assets = assets;
            _mainCamera = Camera.main;
            _cubeRenderer = new CubeRenderer(player.GetComponent<LineRenderer>(), _mainCamera, placeDistance);
            _mapSynchronization = player.GetComponent<MapSynchronization>();
            _palette = hud.GetComponent<Hud>().palette;
            _gameFactory = gameFactory;
            _inventory = InitializeInventory(player.GetComponent<Player.Inventory>().inventory);
            _maxIndex = Math.Min(_inventory.Count, inventoryView.SlotsCount);
            _boarders = inventoryView.Boarders;
            for (var i = 0; i < _maxIndex; i++)
            {
                inventoryView.SetIconForItem(i, _inventory[i].Icon);
            }

            _inventory[_itemIndex].Select();
        }

        private void AddEventHandlers(InventoryInput inventoryInput)
        {
            inventoryInput.OnFirstActionButtonDown += FirstActionButtonDown;
            inventoryInput.OnSecondActionButtonDown += SecondActionButtonDown;
            inventoryInput.OnFirstActionButtonHold += FirstActionButtonHold;
            inventoryInput.OnScroll += Scroll;
            inventoryInput.OnChangeSlot += ChangeSlotIndex;
        }

        private List<IInventoryItemView> InitializeInventory(List<InventoryItem> items)
        {
            var inventory = new List<IInventoryItemView>();
            foreach (var item in items)
            {
                if (item.itemType == ItemType.Block)
                {
                    inventory.Add(new BlockView(_cubeRenderer, _mapSynchronization, _palette, (BlockItem)item));
                }

                if (item.itemType == ItemType.Brush)
                {
                    inventory.Add(new BrushView(_cubeRenderer, _mapSynchronization, _assets, _palette));
                }

                if (item.itemType == ItemType.PrimaryWeapon)
                {
                    inventory.Add(new GunSystem(_gameFactory, this, _mainCamera, itemPosition, _player, _hud, (PrimaryWeapon)item));
                }
            }

            return inventory;
        }

        private void FirstActionButtonDown()
        {
            if (_inventory[_itemIndex] is ILeftMouseButtonDownHandler)
            {
                ((ILeftMouseButtonDownHandler) _inventory[_itemIndex]).OnLeftMouseButtonDown();
            }
        }

        private void SecondActionButtonDown()
        {
            if (_inventory[_itemIndex] is IRightMouseButtonDownHandler)
            {
                ((IRightMouseButtonDownHandler) _inventory[_itemIndex]).OnRightMouseButtonDown();
            }
        }

        private void FirstActionButtonHold()
        {
            if (_inventory[_itemIndex] is ILeftMouseButtonHoldHandler)
            {
                ((ILeftMouseButtonHoldHandler) _inventory[_itemIndex]).OnLeftMouseButtonHold();
            }
        }

        private void Scroll()
        {
            ChangeSlotIndex((_itemIndex + Math.Sign(Input.GetAxis("Mouse ScrollWheel")) + _maxIndex) % _maxIndex);
        }

        private void Update()
        {
            if (_inventory[_itemIndex] is IUpdated)
            {
                ((IUpdated) _inventory[_itemIndex]).InnerUpdate();
            }
        }


        private void ChangeSlotIndex(int newIndex)
        {
            _boarders[_itemIndex].SetActive(false);
            _inventory[_itemIndex].Unselect();
            _itemIndex = newIndex;
            _inventory[_itemIndex].Select();
            _boarders[_itemIndex].SetActive(true);
        }

        public GunSystem[] GetGunSystems()
        {
            return _inventory.OfType<GunSystem>().ToArray();
        }
    }
}
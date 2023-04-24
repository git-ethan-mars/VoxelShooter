using System;
using System.Collections.Generic;
using Data;
using Infrastructure;
using Infrastructure.Factory;
using Infrastructure.Services.Input;
using PlayerLogic;
using Rendering;
using UI;
using UnityEngine;

namespace Inventory
{
    public class InventoryController : MonoBehaviour, ICoroutineRunner
    {
        [SerializeField] private InventoryView inventoryView;
        private Transform _itemPosition;
        private IInputService _inputService;
        private List<IInventoryItemView> _inventoryHandlers;
        private int _itemIndex;
        private int _maxIndex;
        private GameObject[] _boarders;
        private CubeRenderer _cubeRenderer;
        private Camera _mainCamera;
        private MapSynchronization _mapSynchronization;
        private GameObject _palette;
        private IGameFactory _gameFactory;
        private GameObject _player;
        private GameObject _hud;
        private PlayerCharacteristic _characteristic;

        public void Construct(IInputService inputService, IGameFactory gameFactory, GameObject hud, GameObject player)
        {
            AddEventHandlers(player.GetComponent<InventoryInput>());
            _player = player;
            _hud = hud;
            _mainCamera = Camera.main;
            _cubeRenderer = new CubeRenderer(player.GetComponent<LineRenderer>(), _mainCamera, player.GetComponent<Player>().placeDistance);
            _itemPosition = player.GetComponent<Player>().itemPosition;
            _mapSynchronization = player.GetComponent<MapSynchronization>();
            _palette = hud.GetComponent<Hud>().palette;
            _gameFactory = gameFactory;
            _inputService = inputService;
            _inventoryHandlers = InitializeInventoryViews(player.GetComponent<PlayerLogic.Inventory>().inventory);
            _maxIndex = Math.Min(_inventoryHandlers.Count, inventoryView.SlotsCount);
            _boarders = inventoryView.Boarders;
            for (var i = 0; i < _maxIndex; i++)
            {
                inventoryView.SetIconForItem(i, _inventoryHandlers[i].Icon);
            }

            _inventoryHandlers[_itemIndex].Select();
        }

        private void AddEventHandlers(InventoryInput inventoryInput)
        {
            inventoryInput.OnFirstActionButtonDown += FirstActionButtonDown;
            inventoryInput.OnSecondActionButtonDown += SecondActionButtonDown;
            inventoryInput.OnFirstActionButtonHold += FirstActionButtonHold;
            inventoryInput.OnScroll += Scroll;
            inventoryInput.OnChangeSlot += ChangeSlotIndex;
        }

        private List<IInventoryItemView> InitializeInventoryViews(List<InventoryItem> items)
        {
            var inventory = new List<IInventoryItemView>();
            foreach (var item in items)
            {
                if (item.itemType == ItemType.Block)
                {
                    inventory.Add(new BlockView(_gameFactory, _cubeRenderer, _mapSynchronization, _hud, (BlockItem)item, _player));
                }

                if (item.itemType == ItemType.Brush)
                {
                    inventory.Add(new BrushView(_gameFactory, _cubeRenderer, _mapSynchronization, _palette, (BrushItem)item, _player));
                }

                if (item.itemType == ItemType.SpawnPoint)
                {
                    inventory.Add(new SpawnPointView(_cubeRenderer, _mapSynchronization, (SpawnPointItem)item));
                }
            }

            foreach (var (_,weapon) in _player.GetComponent<PlayerLogic.Inventory>().Weapons)
            {
                inventory.Add(new GunSystem(_gameFactory, _inputService, _mainCamera, _itemPosition, _player, _hud, weapon));
            }

            return inventory;
        }

        private void FirstActionButtonDown()
        {
            if (_inventoryHandlers[_itemIndex] is ILeftMouseButtonDownHandler)
            {
                ((ILeftMouseButtonDownHandler) _inventoryHandlers[_itemIndex]).OnLeftMouseButtonDown();
            }
        }

        private void SecondActionButtonDown()
        {
            if (_inventoryHandlers[_itemIndex] is IRightMouseButtonDownHandler)
            {
                ((IRightMouseButtonDownHandler) _inventoryHandlers[_itemIndex]).OnRightMouseButtonDown();
            }
        }

        private void FirstActionButtonHold()
        {
            if (_inventoryHandlers[_itemIndex] is ILeftMouseButtonHoldHandler)
            {
                ((ILeftMouseButtonHoldHandler) _inventoryHandlers[_itemIndex]).OnLeftMouseButtonHold();
            }
        }

        private void Scroll()
        {
            ChangeSlotIndex((_itemIndex + Math.Sign(_inputService.GetScrollSpeed()) + _maxIndex) % _maxIndex);
        }

        private void Update()
        {
            if (_inventoryHandlers[_itemIndex] is IUpdated)
            {
                ((IUpdated) _inventoryHandlers[_itemIndex]).InnerUpdate();
            }
        }


        private void ChangeSlotIndex(int newIndex)
        {
            if (newIndex >= _maxIndex) return;
            _boarders[_itemIndex].SetActive(false);
            _inventoryHandlers[_itemIndex].Unselect();
            _itemIndex = newIndex;
            _inventoryHandlers[_itemIndex].Select();
            _boarders[_itemIndex].SetActive(true);
        }
    }
}
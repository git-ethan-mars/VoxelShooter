using System;
using System.Collections.Generic;
using Data;
using Infrastructure;
using Infrastructure.Factory;
using Infrastructure.Services.Input;
using Mirror;
using Networking.Messages;
using Networking.Synchronization;
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
        private Dictionary<int, IInventoryItemView> _inventoryHandlerByItemId;
        private int _itemIndex;
        private int _maxIndex;
        private GameObject[] _boarders;
        private CubeRenderer _cubeRenderer;
        private Camera _mainCamera;
        private MapSynchronization _mapSynchronization;
        private GameObject _palette;
        private InventoryModelFactory _modelFactory;
        private GameObject _player;
        private GameObject _hud;
        private PlayerCharacteristic _characteristic;
        private Raycaster _raycaster;
        private List<IInventoryItemView> _inventory;
        private TransparentMeshRenderer _transparentMeshFactory;

        public void Construct(IInputService inputService, GameObject hud,
            GameObject player)
        {
            AddEventHandlers(player.GetComponent<InventoryInput>());
            _player = player;
            _hud = hud;
            _mainCamera = Camera.main;
            _transparentMeshFactory = new TransparentMeshRenderer();
            _inventory = new List<IInventoryItemView>();
            _raycaster = new Raycaster(_mainCamera, player.GetComponent<Player>().placeDistance);
            _itemPosition = player.GetComponent<Player>().itemPosition;
            _mapSynchronization = player.GetComponent<MapSynchronization>();
            _palette = hud.GetComponent<Hud>().palette;
            _modelFactory = new InventoryModelFactory();
            _inputService = inputService;
            InitializeInventoryViews(player.GetComponent<PlayerLogic.Inventory>().inventory);
            _maxIndex = Math.Min(_inventoryHandlerByItemId.Count, inventoryView.SlotsCount);
            _boarders = inventoryView.Boarders;
            for (var i = 0; i < _maxIndex; i++)
            {
                inventoryView.SetIconForItem(i, _inventory[i].Icon);
            }

            ChangeSlotIndex(_itemIndex);
            NetworkClient.RegisterHandler<ItemUseResult>(OnItemUse);
            NetworkClient.RegisterHandler<ReloadResult>(OnReloadResult);
            NetworkClient.RegisterHandler<ShootResult>(OnShootResult);
        }

        public void OnDestroy()
        {
            NetworkClient.UnregisterHandler<ItemUseResult>();
            NetworkClient.UnregisterHandler<ReloadResult>();
            NetworkClient.UnregisterHandler<ShootResult>();
        }

        private void AddEventHandlers(InventoryInput inventoryInput)
        {
            inventoryInput.OnFirstActionButtonDown += FirstActionButtonDown;
            inventoryInput.OnSecondActionButtonDown += SecondActionButtonDown;
            inventoryInput.OnFirstActionButtonHold += FirstActionButtonHold;
            inventoryInput.OnScroll += Scroll;
            inventoryInput.OnChangeSlot += ChangeSlotIndex;
        }

        private void InitializeInventoryViews(List<InventoryItem> items)
        {
            _inventoryHandlerByItemId = new Dictionary<int, IInventoryItemView>();
            foreach (var item in items)
            {
                if (item.itemType == ItemType.RangeWeapon)
                {
                    var handler = new RangeWeaponView(_modelFactory, _inputService, _mainCamera, _itemPosition, _player,
                        _hud, new RangeWeaponData((RangeWeaponItem)item));
                    _inventory.Add(handler); 
                    _inventoryHandlerByItemId[item.id] = handler;
                }

                if (item.itemType == ItemType.MeleeWeapon)
                {
                    var handler =  new MeleeWeaponView(_modelFactory, _mainCamera, _itemPosition,
                        _player, new MeleeWeaponData((MeleeWeaponItem) item), _player.GetComponent<LineRenderer>());
                    _inventory.Add(handler); 
                    _inventoryHandlerByItemId[item.id] = handler;
                }
                
                if (item.itemType == ItemType.Block)
                {
                    var handler = new BlockView(_hud,
                        (BlockItem) item, _transparentMeshFactory, _raycaster);
                    _inventory.Add(handler); 
                    _inventoryHandlerByItemId[item.id] = handler;
                }

                if (item.itemType == ItemType.Brush)
                {
                    var handler =new BrushView(_modelFactory, _cubeRenderer, _mapSynchronization,
                        _palette,
                        (BrushItem) item, _player);
                    _inventory.Add(handler); 
                    _inventoryHandlerByItemId[item.id] = handler;
                }

                if (item.itemType == ItemType.SpawnPoint)
                {
                    var handler = _inventoryHandlerByItemId[item.id] =
                        new SpawnPointView(_cubeRenderer, _mapSynchronization, (SpawnPointItem) item);
                    _inventory.Add(handler); 
                    _inventoryHandlerByItemId[item.id] = handler;
                    
                }

                if (item.itemType == ItemType.Tnt)
                {
                    var handler = new TntView(_raycaster, (TntItem) item, _hud.GetComponent<Hud>(), _transparentMeshFactory) ;
                    _inventory.Add(handler); 
                    _inventoryHandlerByItemId[item.id] = handler;
                }
            }
        }

        private void FirstActionButtonDown()
        {
            if (_inventory[_itemIndex] is ILeftMouseButtonDownHandler)
            {
                ((ILeftMouseButtonDownHandler) (_inventory[_itemIndex])).OnLeftMouseButtonDown();
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
            ChangeSlotIndex((_itemIndex + Math.Sign(_inputService.GetScrollSpeed()) + _maxIndex) % _maxIndex);
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
            if (newIndex >= _maxIndex) return;
            _boarders[_itemIndex].SetActive(false);
            _inventory[_itemIndex].Unselect();
            _itemIndex = newIndex;
            _inventory[_itemIndex].Select();
            _boarders[_itemIndex].SetActive(true);
        }

        private void OnItemUse(ItemUseResult message)
        {
            ((IConsumable) _inventoryHandlerByItemId[message.ItemId]).Count = message.Count;
            ((IConsumable) _inventoryHandlerByItemId[message.ItemId]).OnCountChanged();
        }

        private void OnReloadResult(ReloadResult message)
        {
            var reloading = (IReloading) _inventoryHandlerByItemId[message.WeaponId];
            reloading.TotalBullets = message.TotalBullets;
            reloading.BulletsInMagazine = message.BulletsInMagazine;
            reloading.OnReloadResult();
        }

        private void OnShootResult(ShootResult message)
        {
            var shooting = (IShooting) _inventoryHandlerByItemId[message.WeaponId];
            shooting.BulletsInMagazine = message.BulletsInMagazine;
            shooting.OnShootResult();
        }
    }
}
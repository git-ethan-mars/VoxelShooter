using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Infrastructure;
using Infrastructure.AssetManagement;
using Infrastructure.Factory;
using Infrastructure.Services;
using Infrastructure.Services.Input;
using Infrastructure.Services.StaticData;
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
        private TransparentMeshPool _transparentMeshPool;
        private List<Slot> _slots;
        private IStaticDataService _staticData;

        public void Construct(IInputService inputService, IAssetProvider assets, IStaticDataService staticData, GameObject hud,
            GameObject player)
        {
            AddEventHandlers(player.GetComponent<InventoryInput>());
            _staticData = staticData;
            _player = player;
            _hud = hud;
            _mainCamera = Camera.main;
            _transparentMeshPool = new TransparentMeshPool(assets);
            _raycaster = new Raycaster(_mainCamera, player.GetComponent<Player>().placeDistance);
            _itemPosition = player.GetComponent<Player>().itemPosition;
            _mapSynchronization = player.GetComponent<MapSynchronization>();
            _palette = hud.GetComponent<Hud>().palette;
            _modelFactory = new InventoryModelFactory();
            _inputService = inputService;
            InitializeInventoryViews(player.GetComponent<PlayerLogic.Inventory>().ItemIds);
            _maxIndex = Math.Min(_slots.Count, inventoryView.SlotsCount);
            _boarders = inventoryView.Boarders;
            for (var i = 0; i < _maxIndex; i++)
            {
                inventoryView.SetIconForItem(i, _slots[i].ItemHandler.Icon);
            }

            SendChangeSlotRequest(_itemIndex);
            NetworkClient.RegisterHandler<ItemUseResult>(OnItemUse);
            NetworkClient.RegisterHandler<ReloadResult>(OnReloadResult);
            NetworkClient.RegisterHandler<ShootResult>(OnShootResult);
            NetworkClient.RegisterHandler<ChangeSlotResult>(OnChangeSlotResult);
        }

        public void OnDestroy()
        {
            _transparentMeshPool.CleanPool();
            NetworkClient.UnregisterHandler<ItemUseResult>();
            NetworkClient.UnregisterHandler<ReloadResult>();
            NetworkClient.UnregisterHandler<ShootResult>();
            NetworkClient.UnregisterHandler<ChangeSlotResult>();
        }

        private void AddEventHandlers(InventoryInput inventoryInput)
        {
            inventoryInput.OnFirstActionButtonDown += FirstActionButtonDown;
            inventoryInput.OnSecondActionButtonDown += SecondActionButtonDown;
            inventoryInput.OnFirstActionButtonHold += FirstActionButtonHold;
            inventoryInput.OnScroll +=
                () => SendChangeSlotRequest(_itemIndex + Math.Sign(_inputService.GetScrollSpeed()));
            inventoryInput.OnChangeSlot += SendChangeSlotRequest;
        }

        private void InitializeInventoryViews(IEnumerable<int> ids)
        {
            _slots = new List<Slot>();
            var items = ids.Select((id) => _staticData.GetItem(id));
            foreach (var item in items)
            {
                if (item.itemType == ItemType.RangeWeapon)
                {
                    var handler = new RangeWeaponView(_modelFactory, _inputService, _mainCamera, _itemPosition, _player,
                        _hud, new RangeWeaponData((RangeWeaponItem) item));
                    _slots.Add(new Slot(item, handler));
                }

                if (item.itemType == ItemType.MeleeWeapon)
                {
                    var handler = new MeleeWeaponView(_modelFactory, _mainCamera, _itemPosition,
                        _player, new MeleeWeaponData((MeleeWeaponItem) item), _player.GetComponent<LineRenderer>());
                    _slots.Add(new Slot(item, handler));
                }

                if (item.itemType == ItemType.Block)
                {
                    var handler = new BlockView(_hud,
                        (BlockItem) item, _transparentMeshPool, _raycaster, _player);
                    _slots.Add(new Slot(item, handler));
                }

                if (item.itemType == ItemType.Brush)
                {
                    var handler = new BrushView(_modelFactory, _cubeRenderer, _mapSynchronization,
                        _palette,
                        (BrushItem) item, _player);
                    _slots.Add(new Slot(item, handler));
                }

                if (item.itemType == ItemType.SpawnPoint)
                {
                    var handler =
                        new SpawnPointView(_cubeRenderer, _mapSynchronization, (SpawnPointItem) item);
                    _slots.Add(new Slot(item, handler));
                }

                if (item.itemType == ItemType.Tnt)
                {
                    var handler = new TntView(_raycaster, (TntItem) item, _hud.GetComponent<Hud>(),
                        _transparentMeshPool);
                    _slots.Add(new Slot(item, handler));
                }

                if (item.itemType == ItemType.Grenade)
                {
                    var handler = new GrenadeView(_raycaster, (GrenadeItem) item, _hud.GetComponent<Hud>());
                    _slots.Add(new Slot(item, handler));
                }
            }
        }

        private void FirstActionButtonDown()
        {
            if (_slots[_itemIndex].ItemHandler is ILeftMouseButtonDownHandler)
            {
                ((ILeftMouseButtonDownHandler) _slots[_itemIndex].ItemHandler).OnLeftMouseButtonDown();
            }
        }

        private void SecondActionButtonDown()
        {
            if (_slots[_itemIndex].ItemHandler is IRightMouseButtonDownHandler)
            {
                ((IRightMouseButtonDownHandler) _slots[_itemIndex].ItemHandler).OnRightMouseButtonDown();
            }
        }

        private void FirstActionButtonHold()
        {
            if (_slots[_itemIndex].ItemHandler is ILeftMouseButtonHoldHandler)
            {
                ((ILeftMouseButtonHoldHandler) _slots[_itemIndex].ItemHandler).OnLeftMouseButtonHold();
            }
        }


        private void Update()
        {
            if (_slots[_itemIndex].ItemHandler is IUpdated)
            {
                ((IUpdated) _slots[_itemIndex].ItemHandler).InnerUpdate();
            }
        }

        private void SendChangeSlotRequest(int index)
        {
            if (index >= _maxIndex || index < 0) return;
            NetworkClient.Send(new ChangeSlotRequest(index % _maxIndex));
        }


        private void OnChangeSlotResult(ChangeSlotResult message)
        {
            _boarders[_itemIndex].SetActive(false);
            _slots[_itemIndex].ItemHandler.Unselect();
            _itemIndex = message.Index;
            _slots[_itemIndex].ItemHandler.Select();
            _boarders[_itemIndex].SetActive(true);
        }

        private void OnItemUse(ItemUseResult message)
        {
            ((IConsumable) _slots.Find(slot => slot.InventoryItem.id == message.ItemId).ItemHandler).Count =
                message.Count;
            ((IConsumable) _slots.Find(slot => slot.InventoryItem.id == message.ItemId).ItemHandler).OnCountChanged();
        }

        private void OnReloadResult(ReloadResult message)
        {
            var reloading = (IReloading) _slots.Find(slot => slot.InventoryItem.id == message.WeaponId).ItemHandler;
            reloading.TotalBullets = message.TotalBullets;
            reloading.BulletsInMagazine = message.BulletsInMagazine;
            reloading.OnReloadResult();
        }

        private void OnShootResult(ShootResult message)
        {
            var shooting = (IShooting) _slots.Find(slot => slot.InventoryItem.id == message.WeaponId).ItemHandler;
            shooting.BulletsInMagazine = message.BulletsInMagazine;
            shooting.OnShootResult();
        }
    }
}
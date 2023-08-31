using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Infrastructure;
using Infrastructure.AssetManagement;
using Infrastructure.Services.Input;
using Infrastructure.Services.StaticData;
using Mirror;
using Networking.Messages;
using Networking.Messages.Requests;
using Networking.Messages.Responses;
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
        private IInputService _inputService;
        private int _itemIndex;
        private int _maxIndex;
        private GameObject[] _boarders;
        private CubeRenderer _cubeRenderer;
        private Camera _mainCamera;
        private MapSynchronization _mapSynchronization;
        private GameObject _palette;
        private Player _player;
        private Hud _hud;
        private PlayerCharacteristic _characteristic;
        private TransparentMeshPool _transparentMeshPool;
        private List<Slot> _slots;
        private IStaticDataService _staticData;
        private Raycaster _rayCaster;

        public void Construct(IInputService inputService, IAssetProvider assets, IStaticDataService staticData,
            GameObject hud,
            GameObject player)
        {
            AddEventHandlers(player.GetComponent<InventoryInput>());
            _staticData = staticData;
            _player = player.GetComponent<Player>();
            _rayCaster = new Raycaster(Camera.main);
            _transparentMeshPool = new TransparentMeshPool(assets);
            _mapSynchronization = player.GetComponent<MapSynchronization>();
            _hud = hud.GetComponent<Hud>();
            _palette = _hud.palette;
            _inputService = inputService;
            InitializeInventoryViews(player.GetComponent<PlayerLogic.Inventory>().ItemIds);
            _maxIndex = Math.Min(_slots.Count, inventoryView.SlotsCount);
            _boarders = inventoryView.Boarders;
            for (var i = 0; i < _maxIndex; i++)
            {
                inventoryView.SetIconForItem(i, _slots[i].ItemHandler.Icon);
            }

            SendChangeSlotRequest(_itemIndex);
            NetworkClient.RegisterHandler<ItemUseResponse>(OnItemUseResponse);
            NetworkClient.RegisterHandler<ReloadResponse>(OnReloadResponse);
            NetworkClient.RegisterHandler<ShootResponse>(OnShootResponse);
            NetworkClient.RegisterHandler<ChangeSlotResponse>(OnChangeSlotResponse);
        }

        public void OnDestroy()
        {
            _transparentMeshPool.CleanPool();
            NetworkClient.UnregisterHandler<ItemUseResponse>();
            NetworkClient.UnregisterHandler<ReloadResponse>();
            NetworkClient.UnregisterHandler<ShootResponse>();
            NetworkClient.UnregisterHandler<ChangeSlotResponse>();
        }

        private void AddEventHandlers(InventoryInput inventoryInput)
        {
            inventoryInput.OnFirstActionButtonDown += FirstActionButtonDown;
            inventoryInput.OnFirstActionButtonUp += FirstActionButtonUp;
            inventoryInput.OnFirstActionButtonHold += FirstActionButtonHold;

            inventoryInput.OnSecondActionButtonUp += SecondActionButtonUp;
            inventoryInput.OnSecondActionButtonDown += SecondActionButtonDown;

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
                    var handler = new RangeWeaponView(_inputService, Camera.main, _player,
                        _hud, new RangeWeaponData((RangeWeaponItem) item));
                    _slots.Add(new Slot(item, handler));
                }

                if (item.itemType == ItemType.MeleeWeapon)
                {
                    var handler = new MeleeWeaponView(_rayCaster,
                        _player, new MeleeWeaponData((MeleeWeaponItem) item));
                    _slots.Add(new Slot(item, handler));
                }

                if (item.itemType == ItemType.Block)
                {
                    var handler = new BlockView(_rayCaster, _hud,
                        (BlockItem) item, _transparentMeshPool, _player);
                    _slots.Add(new Slot(item, handler));
                }

                if (item.itemType == ItemType.Brush)
                {
                    var handler = new BrushView(_cubeRenderer,
                        _palette,
                        (BrushItem) item);
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
                    var handler = new TntView(_rayCaster, (TntItem) item, _hud,
                        _transparentMeshPool, _player);
                    _slots.Add(new Slot(item, handler));
                }

                if (item.itemType == ItemType.Grenade)
                {
                    var handler = new GrenadeView(_rayCaster, (GrenadeItem) item, _hud.GetComponent<Hud>());
                    _slots.Add(new Slot(item, handler));
                }

                if (item.itemType == ItemType.RocketLauncher)
                {
                    var handler = new RocketLauncherView(_rayCaster, (RocketLauncherItem) item,
                        _hud.GetComponent<Hud>());
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

        private void SecondActionButtonUp()
        {
            if (_slots[_itemIndex].ItemHandler is IRightMouseButtonUpHandler)
            {
                ((IRightMouseButtonUpHandler) _slots[_itemIndex].ItemHandler).OnRightMouseButtonUp();
            }
        }

        private void FirstActionButtonUp()
        {
            if (_slots[_itemIndex].ItemHandler is ILeftMouseButtonUpHandler)
            {
                ((ILeftMouseButtonUpHandler) _slots[_itemIndex].ItemHandler).OnLeftMouseButtonUp();
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
            NetworkClient.Send(new ChangeSlotRequest((index + _maxIndex) % _maxIndex));
        }


        private void OnChangeSlotResponse(ChangeSlotResponse message)
        {
            _boarders[_itemIndex].SetActive(false);
            _slots[_itemIndex].ItemHandler.Unselect();
            _itemIndex = message.Index;
            _slots[_itemIndex].ItemHandler.Select();
            _boarders[_itemIndex].SetActive(true);
        }

        private void OnItemUseResponse(ItemUseResponse message)
        {
            ((IConsumable) _slots.Find(slot => slot.InventoryItem.id == message.ItemId).ItemHandler).Count =
                message.Count;
            ((IConsumable) _slots.Find(slot => slot.InventoryItem.id == message.ItemId).ItemHandler).OnCountChanged();
        }

        private void OnReloadResponse(ReloadResponse message)
        {
            var reloading = (IReloading) _slots.Find(slot => slot.InventoryItem.id == message.WeaponId).ItemHandler;
            reloading.TotalBullets = message.TotalBullets;
            reloading.BulletsInMagazine = message.BulletsInMagazine;
            reloading.OnReloadResult();
        }

        private void OnShootResponse(ShootResponse message)
        {
            var shooting = (IShooting) _slots.Find(slot => slot.InventoryItem.id == message.WeaponId).ItemHandler;
            shooting.BulletsInMagazine = message.BulletsInMagazine;
            shooting.OnShootResult();
        }
    }
}
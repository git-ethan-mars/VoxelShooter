using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Infrastructure;
using Infrastructure.AssetManagement;
using Infrastructure.Services.Input;
using Infrastructure.Services.StaticData;
using Mirror;
using Networking.MessageHandlers.ResponseHandler;
using Networking.Messages.Requests;
using PlayerLogic;
using Rendering;
using UI;
using UnityEngine;

namespace Inventory
{
    public class InventoryController : MonoBehaviour, ICoroutineRunner
    {
        [SerializeField]
        private InventoryView inventoryView;

        public List<Slot> Slots { get; private set; }
        public int ItemIndex { get; set; }
        public GameObject[] Boarders { get; private set; }

        private IInputService _inputService;
        private int _maxIndex;
        private CubeRenderer _cubeRenderer;
        private Camera _mainCamera;
        private Player _player;
        private Hud _hud;
        private PlayerCharacteristic _characteristic;
        private TransparentMeshPool _transparentMeshPool;
        private IStaticDataService _staticData;
        private Raycaster _rayCaster;
        private ItemUseHandler _itemUseHandler;
        private ReloadResultHandler _reloadHandler;
        private ShootResultHandler _shootHandler;
        private ChangeSlotResultHandler _changeSlotHandler;

        public void Construct(IInputService inputService, IAssetProvider assets, IStaticDataService staticData,
            Hud hud,
            Player player)
        {
            _inputService = inputService;
            _staticData = staticData;
            _hud = hud;
            _player = player;
            _rayCaster = new Raycaster(Camera.main);
            _transparentMeshPool = new TransparentMeshPool(assets);
            AddEventHandlers(player.InventoryInput);
            InitializeInventoryViews(_player.ItemsIds);
            Boarders = inventoryView.Boarders;
            for (var i = 0; i < _maxIndex; i++)
            {
                inventoryView.SetIconForItem(i, Slots[i].ItemHandler.Icon);
            }

            SendChangeSlotRequest(ItemIndex);
            _itemUseHandler = new ItemUseHandler(Slots);
            _itemUseHandler.Register();
            _reloadHandler = new ReloadResultHandler(Slots);
            _reloadHandler.Register();
            _shootHandler = new ShootResultHandler(Slots);
            _shootHandler.Register();
            _changeSlotHandler = new ChangeSlotResultHandler(this);
            _changeSlotHandler.Register();
        }

        private void OnDestroy()
        {
            _transparentMeshPool.CleanPool();
            _itemUseHandler.Unregister();
            _reloadHandler.Unregister();
            _shootHandler.Unregister();
            _changeSlotHandler.Unregister();
        }

        private void AddEventHandlers(InventoryInput inventoryInput)
        {
            inventoryInput.OnFirstActionButtonDown += FirstActionButtonDown;
            inventoryInput.OnFirstActionButtonUp += FirstActionButtonUp;
            inventoryInput.OnFirstActionButtonHold += FirstActionButtonHold;

            inventoryInput.OnSecondActionButtonUp += SecondActionButtonUp;
            inventoryInput.OnSecondActionButtonDown += SecondActionButtonDown;

            inventoryInput.OnScroll +=
                () => SendChangeSlotRequest(ItemIndex + Math.Sign(_inputService.GetScrollSpeed()));
            inventoryInput.OnChangeSlot += SendChangeSlotRequest;
        }

        private void InitializeInventoryViews(IEnumerable<int> ids)
        {
            Slots = new List<Slot>();
            var items = ids.Select(id => _staticData.GetItem(id));
            foreach (var item in items)
            {
                if (item.itemType == ItemType.RangeWeapon)
                {
                    var handler = new RangeWeaponView(_inputService, Camera.main,
                        _hud, new RangeWeaponData((RangeWeaponItem) item));
                    Slots.Add(new Slot(item, handler));
                }

                if (item.itemType == ItemType.MeleeWeapon)
                {
                    var handler = new MeleeWeaponView(_rayCaster,
                        _player, new MeleeWeaponData((MeleeWeaponItem) item));
                    Slots.Add(new Slot(item, handler));
                }

                if (item.itemType == ItemType.Block)
                {
                    var handler = new BlockView(_rayCaster, _hud,
                        (BlockItem) item, _transparentMeshPool, _player);
                    Slots.Add(new Slot(item, handler));
                }

                if (item.itemType == ItemType.Tnt)
                {
                    var handler = new TntView(_rayCaster, (TntItem) item, _hud,
                        _transparentMeshPool, _player);
                    Slots.Add(new Slot(item, handler));
                }

                if (item.itemType == ItemType.Grenade)
                {
                    var handler = new GrenadeView(_rayCaster, (GrenadeItem) item, _hud);
                    Slots.Add(new Slot(item, handler));
                }

                if (item.itemType == ItemType.RocketLauncher)
                {
                    var handler = new RocketLauncherView(_rayCaster, (RocketLauncherItem) item,
                        _hud);
                    Slots.Add(new Slot(item, handler));
                }
            }

            _maxIndex = Math.Min(Slots.Count, inventoryView.SlotsCount);
        }

        private void FirstActionButtonDown()
        {
            if (Slots[ItemIndex].ItemHandler is ILeftMouseButtonDownHandler)
            {
                ((ILeftMouseButtonDownHandler) Slots[ItemIndex].ItemHandler).OnLeftMouseButtonDown();
            }
        }

        private void SecondActionButtonDown()
        {
            if (Slots[ItemIndex].ItemHandler is IRightMouseButtonDownHandler)
            {
                ((IRightMouseButtonDownHandler) Slots[ItemIndex].ItemHandler).OnRightMouseButtonDown();
            }
        }

        private void SecondActionButtonUp()
        {
            if (Slots[ItemIndex].ItemHandler is IRightMouseButtonUpHandler)
            {
                ((IRightMouseButtonUpHandler) Slots[ItemIndex].ItemHandler).OnRightMouseButtonUp();
            }
        }

        private void FirstActionButtonUp()
        {
            if (Slots[ItemIndex].ItemHandler is ILeftMouseButtonUpHandler)
            {
                ((ILeftMouseButtonUpHandler) Slots[ItemIndex].ItemHandler).OnLeftMouseButtonUp();
            }
        }

        private void FirstActionButtonHold()
        {
            if (Slots[ItemIndex].ItemHandler is ILeftMouseButtonHoldHandler)
            {
                ((ILeftMouseButtonHoldHandler) Slots[ItemIndex].ItemHandler).OnLeftMouseButtonHold();
            }
        }


        private void Update()
        {
            if (Slots[ItemIndex].ItemHandler is IUpdated)
            {
                ((IUpdated) Slots[ItemIndex].ItemHandler).InnerUpdate();
            }
        }

        private void SendChangeSlotRequest(int index)
        {
            NetworkClient.Send(new ChangeSlotRequest((index + _maxIndex) % _maxIndex));
        }
    }
}
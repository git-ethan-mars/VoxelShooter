using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using GamePlay;
using Infrastructure;
using Infrastructure.Factory;
using Infrastructure.Services;
using Mirror;
using UnityEngine;

namespace UI
{
    public class InventoryController : NetworkBehaviour, ICoroutineRunner
    {
        [SerializeField] private float placeDistance;
        [SerializeField] private Transform itemPosition;
        private List<IInventoryItemView> _itemList;
        private int _itemIndex;
        private int _maxIndex;

        private readonly Dictionary<KeyCode, int> _slotIndexByKey = new()
        {
            [KeyCode.Alpha1] = 0,
            [KeyCode.Alpha2] = 1,
            [KeyCode.Alpha3] = 2,
            [KeyCode.Alpha4] = 3,
            [KeyCode.Alpha5] = 4,
        };

        private GameObject[] _boarders;
        private IGameFactory _gameFactory;
        /*
        public void Construct(IGameFactory gameFactory)
        {
            _gameFactory = gameFactory;
            GlobalEvents.OnMapLoaded.AddListener(() => { enabled = true; });
            
            var mainCamera = Camera.main;
            _itemList = new List<IInventoryItemView>
            {
                new BlockView(GetComponent<LineRenderer>(), mainCamera, placeDistance),
                new BrushView(mainCamera, placeDistance),
                new GunSystem(_gameFactory, this, "StaticData/Inventory Items/Rifle", mainCamera, itemPosition,
                    GetComponent<RaycastSynchronization>(), GetComponent<StatSynchronization>())
            };
        }
        */
        public override void OnStartAuthority()
        {
            _gameFactory = AllServices.Container.Single<IGameFactory>();
            GlobalEvents.OnMapLoaded.AddListener(() => { enabled = true; });
            
            var mainCamera = Camera.main;
            _itemList = new List<IInventoryItemView>
            {
                new BlockView(GetComponent<LineRenderer>(), mainCamera, placeDistance),
                new BrushView(mainCamera, placeDistance),
                new GunSystem(_gameFactory, this, "StaticData/Inventory Items/Rifle", mainCamera, itemPosition,
                    GetComponent<RaycastSynchronization>(), GetComponent<StatSynchronization>())
            };
            
            var inventoryViewGameObject = GameObject.Find("Canvas/GamePlay/Inventory");
            inventoryViewGameObject.SetActive(true);
            var inventoryView = inventoryViewGameObject.GetComponent<InventoryView>();
            _maxIndex = Math.Min(_itemList.Count, inventoryView.SlotsCount);
            _boarders = inventoryView.Boarders;
            for (var i = 0; i < _maxIndex; i++)
            {
                inventoryView.SetIconForItem(i, _itemList[i].Icon);
            }

            _itemList[_itemIndex].Select();
        }

        private void Update()
        {
            if (!isLocalPlayer) return;
            if (Input.GetAxis("Mouse ScrollWheel") != 0.0f)
            {
                ChangeSlotIndex((_itemIndex + Math.Sign(Input.GetAxis("Mouse ScrollWheel")) + _maxIndex) % _maxIndex);
            }

            foreach (var (key, index) in _slotIndexByKey)
            {
                if (Input.GetKeyDown(key))
                {
                    ChangeSlotIndex(index);
                }
            }

            if (Input.GetMouseButtonDown(0) && _itemList[_itemIndex] is ILeftMouseButtonDownHandler)
            {
                ((ILeftMouseButtonDownHandler) _itemList[_itemIndex]).OnLeftMouseButtonDown();
            }

            if (Input.GetMouseButtonDown(1) && _itemList[_itemIndex] is IRightMouseButtonDownHandler)
            {
                ((IRightMouseButtonDownHandler) _itemList[_itemIndex]).OnRightMouseButtonDown();
            }

            if (Input.GetMouseButton(0) && _itemList[_itemIndex] is ILeftMouseButtonHoldHandler)
            {
                ((ILeftMouseButtonHoldHandler) _itemList[_itemIndex]).OnLeftMouseButtonHold();
            }

            if (_itemList[_itemIndex] is IUpdated)
            {
                ((IUpdated) _itemList[_itemIndex]).InnerUpdate();
            }
        }

        private void ChangeSlotIndex(int newIndex)
        {
            _boarders[_itemIndex].SetActive(false);
            _itemList[_itemIndex].Unselect();
            _itemIndex = newIndex;
            _itemList[_itemIndex].Select();
            _boarders[_itemIndex].SetActive(true);
        }

        public GunSystem[] GetGunSystems()
        {
            return _itemList.OfType<GunSystem>().ToArray();
        }
    }
}
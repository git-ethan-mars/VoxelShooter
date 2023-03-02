using System;
using System.Collections.Generic;
using Core;
using GamePlay;
using Mirror;
using UnityEngine;

namespace UI
{
    public class InventoryController : NetworkBehaviour
    {
        private List<IInventoryItemView> _itemList;
        private int _itemIndex;
        private int _maxIndex;
        
        public override void OnStartAuthority()
        {
            _itemList = new List<IInventoryItemView>
                {new BlockView(GetComponent<BlockPlacement>()), new BrushView(GetComponent<ColoringBrush>())};
            var inventoryViewGameObject = GameObject.Find("Canvas/GamePlay/Inventory");
            inventoryViewGameObject.SetActive(true);
            var inventoryView = inventoryViewGameObject.GetComponent<InventoryView>();
            _maxIndex = Math.Min(_itemList.Count, inventoryView.SlotsCount);
            for (var i = 0; i < _maxIndex; i++)
            {
                inventoryView.SetIconForItem(i,_itemList[i].Icon);
                inventoryView.SetPointer(i, _itemList[i]);
            }
            _itemList[_itemIndex].Select();

        }
        private void Update()
        {
            if (!isLocalPlayer) return;
            if (Input.GetAxis("Mouse ScrollWheel") != 0.0f)
            {
                _itemList[_itemIndex].Unselect();
                _itemIndex = (_itemIndex + Math.Sign(Input.GetAxis("Mouse ScrollWheel")) + _maxIndex) % _maxIndex;
                _itemList[_itemIndex].Select();
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _itemList[_itemIndex].Unselect();
                _itemIndex = 0;
                _itemList[_itemIndex].Select();

            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                _itemList[_itemIndex].Unselect();
                _itemIndex = 1;
                _itemList[_itemIndex].Select();

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

            if (Input.GetMouseButton(1) && _itemList[_itemIndex] is IRightMouseButtonHoldHandler)
            {
                ((IRightMouseButtonHoldHandler) _itemList[_itemIndex]).OnRightMouseButtonHold();
            }
        }
    }
}
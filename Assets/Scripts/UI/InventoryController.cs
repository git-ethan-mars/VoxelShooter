using System;
using System.Collections.Generic;
using GamePlay;
using UnityEngine;
using UnityEngine.Serialization;
using Image = UnityEngine.UI.Image;

namespace UI
{
    public class InventoryController : MonoBehaviour
    {
        private List<IInventoryItemView> _itemList;
        [SerializeField] private List<GameObject> slots;
        [SerializeField] private GameObject owner;
        private int _itemIndex;
        private int _maxIndex;

        private void Awake()
        {
            _itemList = new List<IInventoryItemView>()
                {new BlockView(owner.GetComponent<BlockPlacement>()), new BrushView(owner.GetComponent<ColoringBrush>())};
            _maxIndex = Math.Min(_itemList.Count, slots.Count);
            for (var i = 0; i < _maxIndex; i++)
            {
                slots[i].GetComponent<Image>().sprite = _itemList[i].Icon;
                _itemList[i].Pointer = slots[i].transform.Find("Boarder").gameObject;
            }

            _itemList[_itemIndex].Select();
        }

        private void Update()
        {
            if (Input.GetAxis("Mouse ScrollWheel") != 0.0f)
            {
                _itemList[_itemIndex].Unselect();
                _itemIndex = (_itemIndex + Math.Sign(Input.GetAxis("Mouse ScrollWheel")) + _maxIndex) % _maxIndex;
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
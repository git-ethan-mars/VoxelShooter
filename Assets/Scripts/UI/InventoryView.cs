using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InventoryView : MonoBehaviour
    {
        public List<IInventoryItemView> ItemList { get; set; }
        [SerializeField] private List<GameObject> slots;
        private int _itemIndex;
        private int _maxIndex;

        private void Start()
        {
            _maxIndex = Math.Min(ItemList.Count, slots.Count);
            for (var i = 0; i < _maxIndex; i++)
            {
                slots[i].GetComponent<Image>().sprite = ItemList[i].Icon;
                ItemList[i].Pointer = slots[i].transform.Find("Boarder").gameObject;
            }

            ItemList[_itemIndex].Select();
        }

        private void Update()
        {
            if (Input.GetAxis("Mouse ScrollWheel") != 0.0f)
            {
                ItemList[_itemIndex].Unselect();
                _itemIndex = (_itemIndex + Math.Sign(Input.GetAxis("Mouse ScrollWheel")) + _maxIndex) % _maxIndex;
                ItemList[_itemIndex].Select();
            }

            if (Input.GetMouseButtonDown(0) && ItemList[_itemIndex] is ILeftMouseButtonDownHandler)
            {
                ((ILeftMouseButtonDownHandler) ItemList[_itemIndex]).OnLeftMouseButtonDown();
            }

            if (Input.GetMouseButtonDown(1) && ItemList[_itemIndex] is IRightMouseButtonDownHandler)
            {
                ((IRightMouseButtonDownHandler) ItemList[_itemIndex]).OnRightMouseButtonDown();
            }

            if (Input.GetMouseButton(0) && ItemList[_itemIndex] is ILeftMouseButtonHoldHandler)
            {
                ((ILeftMouseButtonHoldHandler) ItemList[_itemIndex]).OnLeftMouseButtonHold();
            }

            if (Input.GetMouseButton(1) && ItemList[_itemIndex] is IRightMouseButtonHoldHandler)
            {
                ((IRightMouseButtonHoldHandler) ItemList[_itemIndex]).OnRightMouseButtonHold();
            }
        }
    }
    
    
}
using System;
using System.Collections.Generic;
using System.Linq;
using GamePlay;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InventoryController : MonoBehaviour
    {
        [SerializeField] private List<InventoryItem> itemList;
        [SerializeField] private List<GameObject> slots;
        private int _itemIndex;
        private int _maxIndex;
        private Dictionary<GameObject, InventoryItem> itemBySlot;

        private void Awake()
        {
            itemBySlot = new Dictionary<GameObject, InventoryItem>();
            GlobalEvents.OnColorBlockChange.AddListener(color=>
            {
                 var newItem = itemList.First((item) => item.color == color);
                 slots[_itemIndex].GetComponent<Image>().sprite = newItem.icon;
                 itemBySlot[slots[_itemIndex]] = newItem;
                 GlobalEvents.SendSlotChoice(newItem);
            });
            _maxIndex = Math.Min(itemList.Count, slots.Count);
            for (var i = 0; i < _maxIndex; i++)
            {
                itemBySlot[slots[i]] = itemList[i];
                slots[i].GetComponent<Image>().sprite = itemList[i].icon;
            }
            slots[_itemIndex].transform.GetChild(0).gameObject.SetActive(true);

        }

        private void Update()
        {
            if (Input.GetAxis("Mouse ScrollWheel") != 0.0f)
            {
                slots[_itemIndex].transform.GetChild(0).gameObject.SetActive(false);
                _itemIndex = (_itemIndex + Math.Sign(Input.GetAxis("Mouse ScrollWheel")) + _maxIndex) % _maxIndex;
                slots[_itemIndex].transform.GetChild(0).gameObject.SetActive(true);
                GlobalEvents.SendSlotChoice(itemBySlot[slots[_itemIndex]]);
            }
        }
    }
}
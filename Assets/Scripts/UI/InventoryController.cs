using System;
using System.Collections.Generic;
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

        private void Start()
        {
            slots[_itemIndex].transform.GetChild(0).gameObject.SetActive(true);
            GlobalEvents.SendBlockChoice(new Block(){Kind = itemList[_itemIndex].kind});
            for (var i = 0; i < itemList.Count; i++)
            {
                slots[i].GetComponent<Image>().sprite = itemList[i].icon;
            }
        }

        private void Update()
        {
            if (Input.GetAxis("Mouse ScrollWheel") != 0.0f)
            {
                slots[_itemIndex].transform.GetChild(0).gameObject.SetActive(false);
                _itemIndex = (_itemIndex + Math.Sign(Input.GetAxis("Mouse ScrollWheel")) + itemList.Count) % itemList.Count;
                slots[_itemIndex].transform.GetChild(0).gameObject.SetActive(true);
                GlobalEvents.SendBlockChoice(new Block(){Kind = itemList[_itemIndex].kind});
            }
        }
    }
}
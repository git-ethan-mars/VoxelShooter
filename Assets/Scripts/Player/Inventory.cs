using System.Collections.Generic;
using Data;
using Infrastructure.Services;
using UnityEngine;

namespace Player
{
    public class Inventory : MonoBehaviour
    {
        public List<InventoryItem> inventory;
        private IStaticDataService _staticData;

        private void Awake()
        {
            _staticData = AllServices.Container.Single<IStaticDataService>();
        }
        public void SetInventory(int[] ids)
        {
            inventory = new List<InventoryItem>();
            for (var i = 0; i < ids.Length; i++)
            {
                inventory.Add(_staticData.GetItem(ids[i]));
            }
        }
    }
}
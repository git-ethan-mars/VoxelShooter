using System.Collections.Generic;
using Data;
using Infrastructure.Services;
using Mirror;
using UnityEngine;

namespace PlayerLogic
{
    public class Inventory : NetworkBehaviour
    {
        public readonly SyncList<int> Ids = new();
        [HideInInspector] public List<InventoryItem> inventory;
        public Dictionary<int,RangeWeaponData> RangeWeapons;
        public Dictionary<int,MeleeWeaponData> MeleeWeapons;
        private IStaticDataService _staticData;
        
        private void Awake()
        {
            _staticData = AllServices.Container.Single<IStaticDataService>();
        }

        public override void OnStartLocalPlayer()
        {
            InitInventory();
        }

        private void InitInventory()
        {
            inventory = new List<InventoryItem>();
            foreach (var id in Ids)
            {
                inventory.Add(_staticData.GetItem(id));
            }
            RangeWeapons = new Dictionary<int, RangeWeaponData>();
            MeleeWeapons = new Dictionary<int, MeleeWeaponData>();
            foreach (var id in Ids)
            {
                var item = _staticData.GetItem(id);
                if (item.itemType == ItemType.RangeWeapon)
                {
                    RangeWeapons[id] = new RangeWeaponData((RangeWeaponItem) item);
                }
                if (item.itemType == ItemType.MeleeWeapon)
                {
                    MeleeWeapons[id] = new MeleeWeaponData((MeleeWeaponItem) item);
                }
            }
        }
    }
}
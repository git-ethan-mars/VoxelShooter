using System.Collections.Generic;
using Data;
using Infrastructure.Services;
using Mirror;

namespace PlayerLogic
{
    public class Inventory : NetworkBehaviour
    {
        public readonly SyncList<int> Ids = new();
        public List<InventoryItem> inventory;
        public Dictionary<int,WeaponData> Weapons;
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
            Weapons = new Dictionary<int, WeaponData>();
            foreach (var id in Ids)
            {
                var item = _staticData.GetItem(id);
                if (item.itemType == ItemType.Weapon)
                {
                    Weapons[id] = new WeaponData((WeaponItem) item);
                }
            }
        }
    }
}
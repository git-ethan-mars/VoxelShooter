using System.Collections.Generic;
using Data;
using Infrastructure.Services;
using Mirror;

namespace Player
{
    public class Inventory : NetworkBehaviour
    {
        public readonly SyncList<int> Ids = new();
        public List<InventoryItem> inventory;
        public Dictionary<int,Weapon> Weapons;
        private IStaticDataService _staticData;
        
        private void Awake()
        {
            _staticData = AllServices.Container.Single<IStaticDataService>();
        }

        public void InitInventory()
        {
            inventory = new List<InventoryItem>();
            foreach (var id in Ids)
            {
                inventory.Add(_staticData.GetItem(id));
            }
            Weapons = new Dictionary<int, Weapon>();
            foreach (var id in Ids)
            {
                var item = _staticData.GetItem(id);
                if (item.itemType == ItemType.PrimaryWeapon)
                {
                    Weapons[id] = new Weapon((PrimaryWeapon) item);
                }
            }
        }
    }
}
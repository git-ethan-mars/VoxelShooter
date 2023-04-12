using System.Collections.Generic;
using System.Linq;
using Infrastructure.Services;

namespace Data
{
    public class PlayerData
    {
        public string NickName;
        public PlayerCharacteristic Characteristic;
        public List<int> ItemIds;
        public readonly Dictionary<int, Weapon> weaponsById;

        public PlayerData(GameClass chosenClass, string nick, IStaticDataService staticDataService)
        {
            NickName = nick;
            Characteristic = staticDataService.GetPlayerCharacteristic(chosenClass);
            ItemIds = staticDataService.GetInventory(chosenClass).Select(item => item.id).ToList();
            weaponsById = new Dictionary<int, Weapon>();
            foreach (var itemId in ItemIds)
            {
                var item = staticDataService.GetItem(itemId);
                if (item.itemType == ItemType.PrimaryWeapon)
                {
                    weaponsById[itemId] = new Weapon((PrimaryWeapon) item);
                }
            }
        }
    }
}
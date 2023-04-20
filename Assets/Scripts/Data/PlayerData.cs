using System.Collections.Generic;
using System.Linq;
using Infrastructure.Services;

namespace Data
{
    public class PlayerData
    {
        public readonly string NickName;
        public readonly GameClass GameClass;
        public int Health;
        public readonly int MaxHealth;
        public readonly Dictionary<int, Weapon> WeaponsById;

        public PlayerData(GameClass chosenClass, string nick, IStaticDataService staticDataService)
        {
            NickName = nick;
            var characteristic = staticDataService.GetPlayerCharacteristic(chosenClass);
            GameClass = chosenClass;
            Health = characteristic.maxHealth;
            MaxHealth = characteristic.maxHealth;
            var itemIds = staticDataService.GetInventory(chosenClass).Select(item => item.id).ToList();
            WeaponsById = new Dictionary<int, Weapon>();
            foreach (var itemId in itemIds)
            {
                var item = staticDataService.GetItem(itemId);
                if (item.itemType == ItemType.Weapon)
                {
                    WeaponsById[itemId] = new Weapon((WeaponItem) item);
                }
            }
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Services;

namespace Data
{
    public class PlayerData
    {
        public string NickName;
        public GameClass gameClass;
        public int health;
        public int maxHealth;
        public float speed;
        public float jumpMultiplier;
        public List<int> ItemIds;
        public readonly Dictionary<int, Weapon> weaponsById;

        public PlayerData(GameClass chosenClass, string nick, IStaticDataService staticDataService)
        {
            NickName = nick;
            var characteristic = staticDataService.GetPlayerCharacteristic(chosenClass);
            gameClass = chosenClass;
            health = characteristic.maxHealth;
            maxHealth = characteristic.maxHealth;
            speed = characteristic.speed;
            jumpMultiplier = characteristic.jumpMultiplier;
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
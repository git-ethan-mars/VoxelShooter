using System.Collections.Generic;
using System.Linq;
using Infrastructure.Services;

namespace Data
{
    public class PlayerData
    {
        public readonly PlayerStatistic PlayerStatistic;
        public readonly string NickName;
        public readonly GameClass GameClass;
        public readonly Dictionary<int, RangeWeaponData> RangeWeaponsById;
        public readonly Dictionary<int, MeleeWeaponData> MeleeWeaponsById;
        public readonly Dictionary<int, int> ItemCountById;
        public int Health;

        public PlayerData(GameClass chosenClass, string nick, IStaticDataService staticDataService)
        {
            NickName = nick;
            GameClass = chosenClass;
            if (GameClass == GameClass.None)
                return;
            PlayerStatistic = new PlayerStatistic();
            Health = staticDataService.GetPlayerCharacteristic(chosenClass).maxHealth;
            RangeWeaponsById = new Dictionary<int, RangeWeaponData>();
            MeleeWeaponsById = new Dictionary<int, MeleeWeaponData>();
            ItemCountById = new Dictionary<int, int>();
            var itemIds = staticDataService.GetInventory(chosenClass).Select(item => item.id).ToList();
            foreach (var itemId in itemIds)
            {
                var item = staticDataService.GetItem(itemId);
                if (item.itemType == ItemType.RangeWeapon)
                {
                    RangeWeaponsById[itemId] = new RangeWeaponData((RangeWeaponItem) item);
                }
                if (item.itemType == ItemType.MeleeWeapon)
                {
                    MeleeWeaponsById[itemId] = new MeleeWeaponData((MeleeWeaponItem) item);
                }

                if (item.itemType == ItemType.Tnt)
                {
                    ItemCountById[itemId] = ((TntItem) item).count;
                    continue;
                }

                if (item.itemType == ItemType.Block)
                {
                    ItemCountById[itemId] = ((BlockItem) item).count;
                    continue;
                }
                ItemCountById[itemId] = 1;
            }
        }
    }
}
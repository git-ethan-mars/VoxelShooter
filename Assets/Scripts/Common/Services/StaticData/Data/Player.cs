using System.Collections.Generic;

namespace Common.StaticData
{
    public class Player
    {
        public readonly ulong ID;
        public readonly string NickName;
        public GameClass GameClass { get; set; } = GameClass.None;
        public PlayerCharacteristic Characteristic { get; private set; }
        public bool IsAlive { get; private set; }
        public int Health { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; private set; }
        public int SelectedSlotIndex { get; set; }
        public List<IItemData> ItemData { get; private set; }
        public IItemData SelectedItemData => ItemData[SelectedSlotIndex];
        public bool HasContinuousSound { get; set; }

        public Player(string nickName)
        {
            NickName = nickName;
        }

        public void ChangeClass(IStaticDataService staticData, GameClass gameClass)
        {
            GameClass = gameClass;
            IsAlive = true;
            Characteristic = staticData.GetPlayerCharacteristic(GameClass);
            Health = Characteristic.maxHealth;
            var items = staticData.GetInventory(GameClass);
            ItemData = new List<IItemData>();
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                
            }
        }

        public void Die()
        {
            IsAlive = false;
            Deaths += 1;
            Characteristic = null;
            Health = 0;
            ItemData = null;
            HasContinuousSound = false;
        }
    }
}
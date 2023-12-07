using System.Collections.Generic;
using Infrastructure.Services.StaticData;
using PlayerLogic.States;
using Steamworks;

namespace Data
{
    public class PlayerData
    {
        public readonly CSteamID SteamID;
        public readonly string NickName;
        public readonly PlayerStateMachine PlayerStateMachine;
        public GameClass GameClass { get; set; } = GameClass.None;
        public PlayerCharacteristic Characteristic { get; set; }
        public bool IsAlive { get; set; }
        public int Health { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int SelectedSlotIndex { get; set; }
        public List<InventoryItem> Items { get; set; }
        public List<IMutableItemData> ItemData { get; set; }
        public Dictionary<InventoryItem, int> CountByItem { get; set; }
        public InventoryItem SelectedItem => Items[SelectedSlotIndex];

        public PlayerData(CSteamID steamID, string nickName, IStaticDataService staticDataService)
        {
            SteamID = steamID;
            NickName = nickName;
            PlayerStateMachine = new PlayerStateMachine(this, staticDataService);
        }
    }
}
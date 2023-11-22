using System.Collections.Generic;
using Infrastructure.Services.StaticData;
using Mirror;
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
        public Dictionary<int, RangeWeaponData> RangeWeaponsById { get; set; }
        public Dictionary<int, MeleeWeaponData> MeleeWeaponsById { get; set; }
        public int SelectedSlotIndex { get; set; }
        public List<int> ItemIds { get; set; }
        public Dictionary<int, int> ItemCountById { get; set; }
        public NetworkConnectionToClient SpectatedPlayer { get; set; }

        public PlayerData(CSteamID steamID, string nickName, IStaticDataService staticDataService)
        {
            SteamID = steamID;
            NickName = nickName;
            PlayerStateMachine = new PlayerStateMachine(this, staticDataService);
        }
    }
}
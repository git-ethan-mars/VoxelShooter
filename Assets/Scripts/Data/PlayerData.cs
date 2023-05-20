using System.Collections.Generic;
using Mirror;
using Steamworks;

namespace Data
{
    public class PlayerData
    {
        public readonly CSteamID SteamID;
        public readonly string NickName;
        public GameClass GameClass = GameClass.None;
        public PlayerCharacteristic Characteristic;
        public bool IsAlive;
        public int Health;
        public int Kills;
        public int Deaths;
        public Dictionary<int, RangeWeaponData> RangeWeaponsById;
        public Dictionary<int, MeleeWeaponData> MeleeWeaponsById;
        public List<int> ItemsId;
        public Dictionary<int, int> ItemCountById;
        public NetworkConnectionToClient SpectatedPlayer;

        public PlayerData(CSteamID steamID, string nickName)
        {
            SteamID = steamID;
            NickName = nickName;
        }
    }
}
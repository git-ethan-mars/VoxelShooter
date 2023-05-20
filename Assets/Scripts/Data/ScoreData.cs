using System;
using Steamworks;

namespace Data
{
    public readonly struct ScoreData : IComparable<ScoreData>
    {
        public readonly CSteamID SteamID;
        public readonly string NickName;
        public readonly int Kills;
        public readonly int Deaths;
        public readonly GameClass GameClass;

        public ScoreData(CSteamID steamID, string nickName, int kills, int deaths, GameClass gameClass)
        {
            SteamID = steamID;
            NickName = nickName;
            Kills = kills;
            Deaths = deaths;
            GameClass = gameClass;
        }

        public int CompareTo(ScoreData other)
        {
            var killsComparison = Kills.CompareTo(other.Kills);
            if (killsComparison != 0) return -killsComparison;
            var steamIDComparison = SteamID.CompareTo(other.SteamID);
            if (steamIDComparison != 0) return steamIDComparison;
            var nickNameComparison = string.Compare(NickName, other.NickName, StringComparison.Ordinal);
            return nickNameComparison;
        }
    }
}
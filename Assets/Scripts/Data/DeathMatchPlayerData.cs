using System;
using UnityEngine;

namespace Data
{
	public readonly struct DeathMatchPlayerData : IEquatable<DeathMatchPlayerData>
	{
		public readonly string NickName;
		public readonly Texture2D Avatar;
		public readonly int Kills;
		public readonly int Deaths;
		public readonly GameClass GameClass;

		public DeathMatchPlayerData(string nickName, Texture2D avatar, int kills = 0, int deaths = 0, GameClass gameClass = GameClass.None)
		{
			NickName = nickName;
			Avatar = avatar;
			Kills = kills;
			Deaths = deaths;
			GameClass = gameClass;
		}

		public DeathMatchPlayerData WithGameClass(GameClass gameClass)
		{
			return new DeathMatchPlayerData(NickName, Avatar, Kills, Deaths, gameClass);
		}

		public bool Equals(DeathMatchPlayerData other)
		{
			return NickName == other.NickName && Equals(Avatar, other.Avatar) && Kills == other.Kills && Deaths == other.Deaths && GameClass == other.GameClass;
		}

		public override bool Equals(object obj)
		{
			return obj is DeathMatchPlayerData other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(NickName, Avatar, Kills, Deaths, (int)GameClass);
		}
	}
}

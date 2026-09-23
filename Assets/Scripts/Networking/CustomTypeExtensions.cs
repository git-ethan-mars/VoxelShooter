using System;
using Data;
using Mirror;

namespace Networking
{
	public static class CustomTypeExtensions
	{
		public static TimeSpan ReadTimeSpan(this NetworkReader reader)
		{
			return new TimeSpan(reader.ReadLong());
		}

		public static void WriteTimeSpan(this NetworkWriter writer, TimeSpan timeSpan)
		{
			writer.WriteLong(timeSpan.Ticks);
		}


		public static DeathMatchPlayerData ReadDeathMatchPlayerData(this NetworkReader reader)
		{
			return new DeathMatchPlayerData(reader.ReadString(), reader.ReadTexture2D());
		}

		public static void WriteDeathMatchPlayerData(this NetworkWriter writer, DeathMatchPlayerData playerData)
		{
			writer.Write(playerData.NickName);
			writer.Write(playerData.Avatar);
			writer.Write(playerData.GameClass);
			writer.Write(playerData.Kills);
			writer.Write(playerData.Deaths);
		}
	}
}

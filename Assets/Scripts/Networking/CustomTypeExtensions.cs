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

		
		public static PlayerData ReadPlayerData(this NetworkReader reader)
		{
			return new PlayerData(reader.ReadString(), reader.ReadTexture2D(),
				(GameClass)reader.ReadInt(), reader.ReadBool(), reader.ReadInt(), 
				reader.ReadInt());
		}

		public static void WritePlayerData(this NetworkWriter writer, PlayerData playerData)
		{
			writer.Write(playerData.NickName);
			writer.Write(playerData.Avatar);
			writer.Write(playerData.GameClass);
			writer.Write(playerData.IsAlive);
			writer.Write(playerData.Kills);
			writer.Write(playerData.Deaths);
		}
	}
}
using UnityEngine;
namespace Data
{
	public class PlayerData
	{
		public readonly string NickName;
		public readonly Texture2D Avatar;
		public GameClass GameClass { get; set; }
		public bool IsAlive { get; set; }
		public int Kills { get; set; }
		public int Deaths { get; set; }

		public PlayerData(string nickName, Texture2D avatar, GameClass gameClass = GameClass.None,
			bool isAlive = false, int kills = 0, int deaths = 0)
		{
			NickName = nickName;
			Avatar = avatar;
			GameClass = gameClass;
			IsAlive = isAlive;
			Kills = kills;
			Deaths = deaths;
		}
	}
}
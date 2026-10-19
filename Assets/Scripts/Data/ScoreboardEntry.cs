namespace Data
{
	public readonly struct ScoreboardEntry
	{
		public readonly int PlayerId;
		public readonly string NickName;
		public readonly GameClass GameClass;
		public readonly int Kills;
		public readonly int Deaths;

		public ScoreboardEntry(int playerId, string nickName, GameClass gameClass = GameClass.None, int kills = 0, int deaths = 0)
		{
			PlayerId = playerId;
			NickName = nickName;
			GameClass = gameClass;
			Kills = kills;
			Deaths = deaths;
		}

		public ScoreboardEntry WithGameClass(GameClass gameClass)
		{
			return new ScoreboardEntry(PlayerId, NickName, gameClass, Kills, Deaths);
		}

		public ScoreboardEntry WithScore(int kills, int deaths)
		{
			return new ScoreboardEntry(PlayerId, NickName, GameClass, kills, deaths);
		}
	}
}

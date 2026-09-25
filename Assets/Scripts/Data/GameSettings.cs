using System;

namespace Data
{
	public struct GameSettings
	{
		public readonly string MapName;
		public readonly TimeSpan GameDuration;
		public readonly TimeSpan RespawnTime;
		public readonly TimeSpan BoxRespawnTime;

		public GameSettings(string mapName, int gameDuration, int characterRespawnTime, int boxSpawnTime)
		{
			MapName = mapName;
			BoxRespawnTime = TimeSpan.FromSeconds(boxSpawnTime);
			// TODO: temporary hardcoded match duration for testing, restore TimeSpan.FromMinutes(gameDuration).
			GameDuration = TimeSpan.FromSeconds(25);
			RespawnTime = TimeSpan.FromSeconds(characterRespawnTime);
		}

		public GameSettings(string mapName, TimeSpan gameDuration, TimeSpan characterRespawnTime, TimeSpan boxSpawnTime)
		{
			MapName = mapName;
			BoxRespawnTime = boxSpawnTime;
			GameDuration = gameDuration;
			RespawnTime = characterRespawnTime;
		}
	}
}

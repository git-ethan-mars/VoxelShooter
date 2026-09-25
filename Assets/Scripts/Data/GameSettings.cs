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
			GameDuration = TimeSpan.FromMinutes(gameDuration);
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

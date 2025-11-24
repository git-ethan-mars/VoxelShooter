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
	}
}
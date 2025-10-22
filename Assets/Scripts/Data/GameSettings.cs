using System;
namespace Data
{
	public class GameSettings
	{
		public readonly string MapName;
		public readonly TimeSpan BoxSpawnTime;
		public readonly TimeSpan GameDuration;
		public readonly TimeSpan SpawnTime;

		public GameSettings(string mapName, int maxDuration, int spawnTime, int boxSpawnTime)
		{
			MapName = mapName;
			BoxSpawnTime = TimeSpan.FromSeconds(boxSpawnTime);
			GameDuration = TimeSpan.FromMinutes(maxDuration);
			SpawnTime = TimeSpan.FromSeconds(spawnTime);
		}
	}
}
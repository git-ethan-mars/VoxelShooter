using System;
namespace Data
{
	public class WorldSettings
	{
		public readonly string MapName;
		public readonly MapConfigure Configure;
		public readonly TimeSpan BoxSpawnTime;
		public readonly TimeSpan GameDuration;
		public readonly TimeSpan SpawnTime;

		public WorldSettings(string mapName, MapConfigure configure, int maxDuration, int spawnTime, int boxSpawnTime)
		{
			MapName = mapName;
			Configure = configure;
			BoxSpawnTime = TimeSpan.FromSeconds(boxSpawnTime);
			GameDuration = TimeSpan.FromMinutes(maxDuration);
			SpawnTime = TimeSpan.FromSeconds(spawnTime);
		}
	}
}
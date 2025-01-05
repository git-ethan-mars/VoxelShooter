namespace GamePlay.Data
{
    public class WorldSettings
    {
        public readonly string MapName;
        public readonly int MaxDuration;
        public readonly int SpawnTime;
        public readonly int BoxSpawnTime;

        public WorldSettings(string mapName, int maxDuration, int spawnTime, int boxSpawnTime)
        {
            MapName = mapName;
            MaxDuration = maxDuration;
            SpawnTime = spawnTime;
            BoxSpawnTime = boxSpawnTime;
        }
    }
}
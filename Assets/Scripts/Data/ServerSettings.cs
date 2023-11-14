namespace Data
{
    public class ServerSettings
    {
        public readonly int MaxDuration;
        public readonly int SpawnTime;
        public readonly int BoxSpawnTime;
        public readonly string MapName;

        public ServerSettings(string mapName, int maxDuration, int spawnTime, int boxSpawnTime)
        {
            MaxDuration = maxDuration;
            SpawnTime = spawnTime;
            BoxSpawnTime = boxSpawnTime;
            MapName = mapName;
        }
    }
}
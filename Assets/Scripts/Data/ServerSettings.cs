namespace Data
{
    public class ServerSettings
    {
        public readonly int MaxDuration;
        public readonly int SpawnTime;
        public readonly string MapName;

        public ServerSettings(int maxDuration, int spawnTime, string mapName)
        {
            MaxDuration = maxDuration;
            SpawnTime = spawnTime;
            MapName = mapName;
        }
    }
}
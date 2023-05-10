namespace Data
{
    public class ServerSettings
    {
        public int MaxDuration;
        public int MaxPlayers;
        public int SpawnTime;
        public readonly string MapName;

        public ServerSettings(int maxDuration, int maxPlayers, int spawnTime, string mapName)
        {
            MaxDuration = maxDuration;
            MaxPlayers = maxPlayers;
            SpawnTime = spawnTime;
            MapName = mapName;
        }
    }
}
namespace Data
{
    public class ServerSettings
    {
        public readonly int MaxDuration;
        public readonly int MaxPlayers;
        public readonly string MapName;
        public ServerSettings(string mapName, int maxDuration, int maxPlayers)
        {
            MapName = mapName;
            MaxDuration = maxDuration;
            MaxPlayers = maxPlayers;
        }
    }
}
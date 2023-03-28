namespace Data
{
    public class MapProgress
    {
        public readonly MapData Data;
        public readonly string Name;

        public MapProgress(MapData data, string mapName)
        {
            Data = data;
            Name = mapName;
        }
    }
}
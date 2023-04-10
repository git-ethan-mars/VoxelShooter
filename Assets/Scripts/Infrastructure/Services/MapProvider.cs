using MapLogic;

namespace Infrastructure.Services
{
    public class MapProvider : IMapProvider
    {
        public MapProvider(Map map)
        {
            Map = map;
        }

        public Map Map { get; set; }
    }
}
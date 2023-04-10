using MapLogic;

namespace Infrastructure.Services
{
    public class MapGeneratorProvider : IMapGeneratorProvider
    {
        public MapGenerator MapGenerator { get; set; }
        public MapGeneratorProvider(MapGenerator mapGenerator)
        {
            MapGenerator = mapGenerator;
        }
    }
}
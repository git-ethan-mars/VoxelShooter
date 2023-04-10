using MapLogic;

namespace Infrastructure.Services
{
    public interface IMapGeneratorProvider : IService
    {
        MapGenerator MapGenerator { get; set; }
    }
}
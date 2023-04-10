using MapLogic;

namespace Infrastructure.Services
{
    public interface IMapProvider : IService
    {
        public Map Map { get; set; }
    }
}
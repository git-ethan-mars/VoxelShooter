using Networking.Synchronization;

namespace Infrastructure.Services
{
    public interface IMapSynchronizationProvider : IService
    {
        MapMessageHandler MapSynchronization { get; set; }
    }
}
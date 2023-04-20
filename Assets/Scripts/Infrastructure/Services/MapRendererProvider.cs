using Networking.Synchronization;

namespace Infrastructure.Services
{
    public class MapSynchronizationProvider : IMapSynchronizationProvider
    {
        public MapMessageHandler MapSynchronization { get; set; }
        public MapSynchronizationProvider(MapMessageHandler mapSynchronization)
        {
            MapSynchronization = mapSynchronization;
        }
    }
}
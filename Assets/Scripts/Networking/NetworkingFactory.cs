using Common.AssetManagement;
using GamePlay;
using GamePlay.Data;
using GamePlay.Entities;
using GamePlay.Factory;
using GamePlay.MapFeatures;
using GamePlay.Services;
using Mirror;
using Networking.Client;
using Networking.Host;
using VoxelMap;
using MirrorClient = Networking.Client.MirrorClient;

namespace Networking
{
    public class NetworkingFactory : INetworkingFactory
    {
        private const string NetworkManagerPath = "Prefabs/Network/LocalNetworkManager";

        private readonly IAssetProvider _assets;
        private readonly IStaticDataService _staticData;
        private readonly IStorageService _storage;
        private readonly IEntityFactory _entityFactory;
        private readonly IParticleFactory _particleFactory;
        private readonly IMeshFactory _meshFactory;
        private readonly IInventoryFactory _inventoryFactory;
        private readonly IMapConfigureLoader _mapConfigureLoader;
        private readonly ICharacterFactory _characterFactory;

        public NetworkingFactory(IAssetProvider assets, IStaticDataService staticData,
            IStorageService storage, ICharacterFactory characterFactory, IEntityFactory entityFactory, IParticleFactory particleFactory,
            IMeshFactory meshFactory, IInventoryFactory inventoryFactory, IMapConfigureLoader mapConfigureLoader)
        {
            _assets = assets;
            _staticData = staticData;
            _storage = storage;
            _characterFactory = characterFactory;
            _entityFactory = entityFactory;
            _particleFactory = particleFactory;
            _meshFactory = meshFactory;
            _inventoryFactory = inventoryFactory;
            _mapConfigureLoader = mapConfigureLoader;
        }

        public IHost CreateHost(WorldSettings worldSettings, MapProvider mapProvider)
        {
            var networkManager = CreateNetworkManager();
            var client = new MirrorClient(networkManager, _assets, _staticData, _storage, _characterFactory,
                _particleFactory, _meshFactory, _inventoryFactory);
            return new MirrorHost(client, networkManager,
                _entityFactory, _characterFactory, _inventoryFactory, _mapConfigureLoader, worldSettings, mapProvider);
        }

        public IClient CreateClient()
        {
            var networkManager = CreateNetworkManager();
            return new MirrorClient(networkManager, _assets, _staticData, _storage, _characterFactory,
                _particleFactory, _meshFactory, _inventoryFactory);
        }

        private NetworkManager CreateNetworkManager()
        {
            return _assets.Instantiate(NetworkManagerPath).GetComponent<NetworkManager>();
        }
    }
}
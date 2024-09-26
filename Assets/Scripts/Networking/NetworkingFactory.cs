using Common.AssetManagement;
using Common.Factory;
using Common.StaticData;
using Common.Storage;
using Entities;
using Infrastructure.Factory;
using Mirror;
using Networking.Client;
using Networking.Host;

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

		public NetworkingFactory(IAssetProvider assets, IStaticDataService staticData,
			IStorageService storage, IEntityFactory entityFactory, IParticleFactory particleFactory, IMeshFactory meshFactory)
		{
			_assets = assets;
			_staticData = staticData;
			_storage = storage;
			_entityFactory = entityFactory;
			_particleFactory = particleFactory;
			_meshFactory = meshFactory;
		}

		public IHost CreateHost(WorldSettings worldSettings)
		{
			var networkManager = CreateNetworkManager();
			var client = new MirrorClient(networkManager, _assets, _staticData, _storage, _entityFactory,
				_particleFactory, _meshFactory);
			return new MirrorHost(client, networkManager, _staticData, _particleFactory,
				_entityFactory, worldSettings);
		}

		public IClient CreateClient()
		{
			var networkManager = CreateNetworkManager();
			return new MirrorClient(networkManager, _assets, _staticData, _storage, _entityFactory,
				_particleFactory, _meshFactory);
		}

		private NetworkManager CreateNetworkManager()
		{
			return _assets.Instantiate(NetworkManagerPath).GetComponent<NetworkManager>();
		}
	}
}
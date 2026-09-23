using Reflex.Core;
using Services;
namespace Networking
{
	public class NetworkFactory : INetworkFactory
	{
		private const string LocalNetworkManagerPath = "Prefabs/Network/LocalNetworkManager";
		private const string SteamNetworkManagerPath = "Prefabs/Network/SteamNetworkManager";

		private readonly IAssetProvider _assets;

		public NetworkFactory(Container container)
		{
			_assets = container.Single<IAssetProvider>();
		}

		public VSNetworkManager CreateNetworkManager()
		{
#if LOCAL_BUILD
			var networkManager = _assets.Instantiate(LocalNetworkManagerPath).GetComponent<VSNetworkManager>();
#else
			var networkManager = _assets.Instantiate(SteamNetworkManagerPath).GetComponent<VSNetworkManager>();
#endif
			return networkManager;
		}
	}
}
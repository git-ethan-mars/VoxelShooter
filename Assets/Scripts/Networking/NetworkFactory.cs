using Reflex.Core;
using Services;
namespace Networking
{
	public class NetworkFactory : INetworkFactory
	{
		private const string NetworkManagerPath = "Prefabs/Network/LocalNetworkManager";

		private readonly IAssetProvider _assets;

		public NetworkFactory(Container container)
		{
			_assets = container.Single<IAssetProvider>();
		}

		public VoxelShooterNetworkManager CreateNetworkManager()
		{
			var networkManager = _assets.Instantiate(NetworkManagerPath).GetComponent<VoxelShooterNetworkManager>();
			return networkManager;
		}
	}
}
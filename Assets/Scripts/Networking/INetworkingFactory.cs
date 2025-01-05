using Common;
using GamePlay.Data;
using Networking.Client;
using Networking.Host;
using VoxelMap;

namespace Networking
{
	public interface INetworkingFactory : IService
	{
		IHost CreateHost(WorldSettings worldSettings, MapProvider mapProvider);
		IClient CreateClient();
	}
}
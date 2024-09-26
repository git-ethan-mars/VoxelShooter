using Common;
using Common.StaticData;
using Networking.Client;
using Networking.Host;

namespace Networking
{
	public interface INetworkingFactory : IService
	{
		IHost CreateHost(WorldSettings worldSettings);
		IClient CreateClient();
	}
}
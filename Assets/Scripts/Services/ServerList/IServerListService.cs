using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
namespace Services
{
	public interface IServerListService
	{
		UniTask<List<ServerInfo>> GetServersAsync(CancellationToken cancellationToken);
		UniTask CreateServerAsync(ServerInfo server, CancellationToken cancellationToken);
		UniTask RemoveServerAsync(ServerInfo server, CancellationToken cancellationToken);
	}
}
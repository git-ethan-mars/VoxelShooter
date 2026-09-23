using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
namespace Services.ServerList
{
	public interface IServerListService
	{
		UniTask<List<Server>> GetServersAsync(CancellationToken cancellationToken);
		UniTask<Server> CreateNewServerAsync(ulong steamIDLobby, int availableSlots,CancellationToken cancellationToken);
		UniTask RemoveServerAsync(long serverID, CancellationToken cancellationToken);
		UniTask UpdateServerAsync(Server server, CancellationToken cancellationToken);
	}
}
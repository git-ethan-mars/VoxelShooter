using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Services
{
	// Failed requests throw Cysharp.Threading.Tasks.UnityWebRequestException (ResponseCode holds the HTTP status).
	public interface IServerListService
	{
		UniTask<List<Server>> GetActiveServersAsync(CancellationToken cancellationToken);
		UniTask<Server> CreateServerAsync(ServerCreateRequest serverCreate, CancellationToken cancellationToken);
		UniTask<Server> UpdateServerAsync(long serverID, ServerUpdateRequest serverUpdate, CancellationToken cancellationToken);
		UniTask RemoveServerAsync(long serverID, CancellationToken cancellationToken);
	}
}

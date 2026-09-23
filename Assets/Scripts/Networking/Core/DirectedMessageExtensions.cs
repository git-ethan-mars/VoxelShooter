using System.Threading;
using Cysharp.Threading.Tasks;
using Mirror;
using R3;

namespace Networking.Core
{
	public static class DirectedMessageExtensions
	{
		public static Observable<DirectedMessage<TResult>> OfMessageType<TResult>(this Observable<DirectedMessage> observable)
			where TResult : struct, NetworkMessage
		{
			return new OfMessageType<TResult>(observable);
		}

		public static UniTask<DirectedMessage<TResult>> FirstAsync<TResult>(this Observable<DirectedMessage> observable, CancellationToken
			cancellationToken = default)
			where TResult : struct, NetworkMessage
		{
			return new OfMessageType<TResult>(observable).FirstAsync(cancellationToken: cancellationToken).AsUniTask();
		}
	}
}

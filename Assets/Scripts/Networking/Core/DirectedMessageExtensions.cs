using System;
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
	}

	internal class OfMessageType<TResult> : Observable<DirectedMessage<TResult>> where TResult : struct, NetworkMessage
	{
		private readonly Observable<DirectedMessage> _source;

		public OfMessageType(Observable<DirectedMessage> source)
		{
			_source = source;
		}

		protected override IDisposable SubscribeCore(Observer<DirectedMessage<TResult>> observer)
		{
			return _source.Subscribe(new _OfMessageType(observer));
		}

		private sealed class _OfMessageType : Observer<DirectedMessage>
		{
			private readonly Observer<DirectedMessage<TResult>> _observer;

			public _OfMessageType(Observer<DirectedMessage<TResult>> observer)
			{
				_observer = observer;
			}

			protected override void OnNextCore(DirectedMessage value)
			{
				if (value.Message is not TResult)
					return;
				_observer.OnNext(new DirectedMessage<TResult>(value.Connection, (TResult)value.Message));

			}

			protected override void OnErrorResumeCore(Exception error) => _observer.OnErrorResume(error);

			protected override void OnCompletedCore(Result result) => _observer.OnCompleted(result);
		}
	}
}
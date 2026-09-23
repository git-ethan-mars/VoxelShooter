using System;
using Mirror;
using R3;

namespace Networking.Core
{
	internal class OfMessageType<TResult> : Observable<DirectedMessage<TResult>> where TResult : struct, NetworkMessage
	{
		private readonly Observable<DirectedMessage> _source;

		public OfMessageType(Observable<DirectedMessage> source)
		{
			_source = source;
		}

		protected override IDisposable SubscribeCore(Observer<DirectedMessage<TResult>> observer)
		{
			return _source.Subscribe(new FilteringObserver(observer));
		}

		private sealed class FilteringObserver : Observer<DirectedMessage>
		{
			private readonly Observer<DirectedMessage<TResult>> _observer;

			public FilteringObserver(Observer<DirectedMessage<TResult>> observer)
			{
				_observer = observer;
			}

			protected override void OnNextCore(DirectedMessage value)
			{
				if (value.Message is not TResult)
				{
					return;
				}

				_observer.OnNext(new DirectedMessage<TResult>(value.Connection, (TResult)value.Message));
			}

			protected override void OnErrorResumeCore(Exception error) => _observer.OnErrorResume(error);

			protected override void OnCompletedCore(Result result) => _observer.OnCompleted(result);
		}
	}
}

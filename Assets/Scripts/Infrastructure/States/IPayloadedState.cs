using Cysharp.Threading.Tasks;

namespace Infrastructure.States
{
	public interface IPayloadedState<TPayload> : IExitableState
	{
		UniTask EnterAsync(TPayload payload);
	}

	public interface IPayloadedState<TPayload1, TPayload2> : IExitableState
	{
		UniTask EnterAsync(TPayload1 payload1, TPayload2 payload2);
	}

	public interface IPayloadedState<TPayload1, TPayload2, TPayload3> : IExitableState
	{
		UniTask EnterAsync(TPayload1 payload1, TPayload2 payload2, TPayload3 payload3);
	}
}

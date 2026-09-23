namespace Infrastructure.States
{
	public interface IPayloadedState<TPayload> : IExitableState
	{
		void Enter(TPayload payload);
	}

	public interface IPayloadedState<TPayload1, TPayload2> : IExitableState
	{
		void Enter(TPayload1 payload1, TPayload2 payload2);
	}

	public interface IPayloadedState<TPayload1, TPayload2, TPayload3> : IExitableState
	{
		void Enter(TPayload1 payload1, TPayload2 payload2, TPayload3 payload3);
	}
}

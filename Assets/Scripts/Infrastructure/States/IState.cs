namespace Infrastructure.States
{
    public interface IState : IExitableState
    {
        void Enter();
    }

    public interface IPayloadedState<TPayload> : IExitableState
    {
        void Enter(TPayload payload);
    }

    public interface IExitableState
    {
        void Exit();
    }

    public interface IPayloadedState<TPayload1, TPayload2, TPayload3> : IExitableState
    {
        void Enter(TPayload1 payload1, TPayload2 payload2, TPayload3 payload3);
    }
}
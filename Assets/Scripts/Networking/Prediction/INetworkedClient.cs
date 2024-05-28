namespace Networking.Prediction
{
    public interface INetworkedClient
    {
        INetworkedClientState LatestServerState { get; }
        uint CurrentTick { get; }
    }
}
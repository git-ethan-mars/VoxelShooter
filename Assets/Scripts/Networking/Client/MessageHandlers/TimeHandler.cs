using Networking.Messages.Responses;

namespace Networking.Client
{
    public partial class MirrorClient : IResponseHandler<GameTimeResponse>, IResponseHandler<RespawnTimeResponse>
    {
        public void OnResponseReceived(GameTimeResponse response)
        {
            GameTimeChanged?.Invoke(response.TimeLeft);
        }

        public void OnResponseReceived(RespawnTimeResponse response)
        {
            RespawnTimeChanged?.Invoke(response.TimeLeft);
        }
    }
}
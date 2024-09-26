using Networking.Messages.Responses;

namespace Networking.Client
{
    public partial class MirrorClient : IResponseHandler<ScoreboardResponse>
    {
        public void OnResponseReceived(ScoreboardResponse message)
        {
            ScoreboardChanged?.Invoke(message.Scores);
        }
    }
}
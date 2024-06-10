using Networking.Messages.Responses;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class MapNameHandler : ResponseHandler<MapNameResponse>
    {
        private readonly IClient _client;

        public MapNameHandler(IClient client)
        {
            _client = client;
        }

        protected override void OnResponseReceived(MapNameResponse response)
        {
            _client.MapLoadingProgress.MapName = response.MapName;
        }
    }
}
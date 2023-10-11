using Networking.Messages.Responses;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class MapNameHandler : ResponseHandler<MapNameResponse>
    {
        private readonly Client _client;

        public MapNameHandler(Client client)
        {
            _client = client;
        }

        protected override void OnResponseReceived(MapNameResponse response)
        {
            _client.Data.MapName = response.MapName;
        }
    }
}
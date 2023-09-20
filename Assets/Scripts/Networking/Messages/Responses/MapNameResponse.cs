using Mirror;

namespace Networking.Messages.Responses
{
    public struct MapNameResponse : NetworkMessage
    {
        public string MapName;

        public MapNameResponse(string mapName)
        {
            MapName = mapName;
        }
    }
}
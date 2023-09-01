using System.Collections.Generic;
using MapLogic;
using Networking.Messages;
using Networking.Messages.Responses;

namespace Networking
{
    public class ClientData
    {
        public IMapProvider MapProvider { get; set; }
        public List<UpdateMapResponse> BufferToUpdateMap { get; set; }
        public List<FallBlockResponse> BufferToFallBlocks { get; set; }
        public ClientData()
        {
            BufferToFallBlocks = new List<FallBlockResponse>();
            BufferToUpdateMap = new List<UpdateMapResponse>();
        }
    }
}
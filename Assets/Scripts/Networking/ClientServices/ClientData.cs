using System.Collections.Generic;
using Networking.Messages.Responses;

namespace Networking
{
    public class ClientData
    {
        public List<UpdateMapResponse> BufferToUpdateMap { get; set; } = new();
        public List<FallBlockResponse> BufferToFallBlocks { get; set; } = new();
        public ClientState State { get; set; } = ClientState.NotConnected;
    }
}
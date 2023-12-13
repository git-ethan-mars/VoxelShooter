using System.Collections.Generic;
using Mirror;
using Networking.Messages.Responses;

namespace Networking
{
    public class MapSplitter
    {
        public DownloadMapResponse[] SplitBytesIntoMessages(byte[] blocks, int maxPacketSize)
        {
            var messages = new List<DownloadMapResponse>();
            for (var i = 0; i < blocks.Length; i += maxPacketSize)
            {
                messages.Add(blocks.Length <= i + maxPacketSize
                    ? new DownloadMapResponse(blocks[i..blocks.Length], 1)
                    : new DownloadMapResponse(blocks[i..(i + maxPacketSize)],
                        (float) (i + maxPacketSize) / blocks.Length));
            }

            return messages.ToArray();
        }

        public void SendMessages(DownloadMapResponse[] messages, NetworkConnectionToClient destination)
        {
            for (var i = 0; i < messages.Length; i++)
            {
                destination.Send(messages[i]);
            }

            NetworkServer.SetClientReady(destination);
        }
    }
}
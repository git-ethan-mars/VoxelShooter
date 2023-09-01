using System.Collections;
using System.Collections.Generic;
using Mirror;
using Networking.Messages;
using Networking.Messages.Responses;
using UnityEngine;

namespace Networking
{
    public class MapSplitter : MessageSplitter<DownloadMapResponse>
    {
        public override DownloadMapResponse[] SplitBytesIntoMessages(byte[] bytes, int maxPacketSize)
        {
            var messages = new List<DownloadMapResponse>();
            for (var i = 0; i < bytes.Length; i += maxPacketSize)
            {
                messages.Add(bytes.Length <= i + maxPacketSize
                    ? new DownloadMapResponse(bytes[i..bytes.Length], 1)
                    : new DownloadMapResponse(bytes[i..(i + maxPacketSize)], (float)(i + maxPacketSize) / bytes.Length));
            }

            return messages.ToArray();
        }

        public override IEnumerator SendMessages(DownloadMapResponse[] messages, NetworkConnectionToClient destination,
            float delayInSeconds)
        {
            for (var i = 0; i < messages.Length; i++)
            {
                destination.Send(messages[i]);
                yield return new WaitForSeconds(delayInSeconds);
            }
        }
    }
}
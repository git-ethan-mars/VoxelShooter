using System.Collections;
using System.Collections.Generic;
using Mirror;
using Networking.Messages;
using UnityEngine;

namespace Networking
{
    public class MapSplitter : MessageSplitter<DownloadMapMessage>
    {

        public override DownloadMapMessage[] SplitBytesIntoMessages(byte[] bytes, int maxPacketSize)
        {
            var messages = new List<DownloadMapMessage>();
            for (var i = 0; i < bytes.Length; i += maxPacketSize)
            {
                messages.Add(bytes.Length <= i + maxPacketSize
                    ? new DownloadMapMessage(bytes[i..bytes.Length], true)
                    : new DownloadMapMessage(bytes[i..(i + maxPacketSize)], false));
            }

            return messages.ToArray();

        }

        public override IEnumerator SendMessages(DownloadMapMessage[] messages, NetworkConnectionToClient destination,
            float delayInSeconds)
        {
            for (var i = 0; i < messages.Length; i++)
            {
                destination.Send(messages[i]);
                yield return new WaitForSeconds(delayInSeconds);
                Debug.Log($"{(int) ((float) i / messages.Length * 100)}%");
            }
        }
    }
}

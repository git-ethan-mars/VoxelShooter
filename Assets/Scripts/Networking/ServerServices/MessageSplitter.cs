using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using Mirror;
using Networking.Messages.Responses;
using UnityEngine;

namespace Networking
{
    public static class MessageSplitter
    {
        public static UpdateMapResponse[] SplitBlocksIntoUpdateMessages(IList<BlockDataWithPosition> blocks,
            int maxPacketSize)
        {
            var messages = new List<UpdateMapResponse>();
            var blockBuffer = new List<BlockDataWithPosition>();
            for (var i = 0; i < blocks.Count; i++)
            {
                blockBuffer.Add(blocks[i]);
                if ((blockBuffer.Count + 1) * sizeof(int) * 4 >= maxPacketSize)
                {
                    messages.Add(new UpdateMapResponse(blocks.ToArray()));
                    blockBuffer.Clear();
                }
            }

            if (blockBuffer.Count > 0)
                messages.Add(new UpdateMapResponse(blocks.ToArray()));

            return messages.ToArray();
        }

        public static FallBlockResponse[] SplitBlocksIntoFallingMessages(IList<BlockDataWithPosition> blocks,
            int maxPacketSize)
        {
            var messages = new List<FallBlockResponse>();
            var blockBuffer = new List<BlockDataWithPosition>();
            for (var i = 0; i < blocks.Count; i++)
            {
                blockBuffer.Add(blocks[i]);
                if ((blockBuffer.Count + 1) * sizeof(int) * 4 >= maxPacketSize)
                {
                    messages.Add(new FallBlockResponse(blocks.ToArray()));
                    blockBuffer.Clear();
                }
            }

            if (blockBuffer.Count > 0)
                messages.Add(new FallBlockResponse(blocks.ToArray()));

            return messages.ToArray();
        }

        public static DownloadMapResponse[] SplitBytesIntoMessages(byte[] blocks, int maxPacketSize)
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

        public static IEnumerator SendMessages<T>(T[] messages, float delayInSeconds,
            bool onlyReady = false, NetworkConnectionToClient connection = null) where T : struct, NetworkMessage
        {
            if (connection is null)
            {
                if (onlyReady)
                {
                    yield return Send(messages, delayInSeconds, message => NetworkServer.SendToReady(message));
                }
                else
                {
                    yield return Send(messages, delayInSeconds, message => NetworkServer.SendToAll(message));
                }
            }
            else
            {
                if (onlyReady && connection.isReady || !onlyReady)
                {
                    yield return Send(messages, delayInSeconds, message => connection.Send(message));
                }
            }
        }

        private static IEnumerator Send<T>(IReadOnlyList<T> messages, float delayInSeconds,
            Action<T> sendDelegate) where T : struct, NetworkMessage
        {
            for (var i = 0; i < messages.Count; i++)
            {
                sendDelegate.Invoke(messages[i]);
                yield return new WaitForSeconds(delayInSeconds);
            }
        }
    }
}
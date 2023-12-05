using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using Mirror;
using Networking.Messages.Responses;
using UnityEngine;

namespace Networking
{
    public class BlockSplitter
    {
        public UpdateMapResponse[] SplitBlocksIntoUpdateMessages(IList<BlockDataWithPosition> blocks,
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

        public FallBlockResponse[] SplitBlocksIntoFallingMessages(IList<BlockDataWithPosition> blocks,
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

        public IEnumerator SendMessages<T>(T[] messages, float delayInSeconds) where T : struct, NetworkMessage
        {
            for (var i = 0; i < messages.Length; i++)
            {
                NetworkServer.SendToAll(messages[i]);
                yield return new WaitForSeconds(delayInSeconds);
            }
        }
    }
}
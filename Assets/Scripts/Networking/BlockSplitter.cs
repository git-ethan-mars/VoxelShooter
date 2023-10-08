using System.Collections;
using System.Collections.Generic;
using Data;
using Mirror;
using Networking.Messages.Responses;
using UnityEngine;

namespace Networking
{
    public class BlockSplitter : MessageSplitter<UpdateMapResponse, Vector3Int, BlockData>
    {
        public override UpdateMapResponse[] SplitBytesIntoMessages(Vector3Int[] positions, BlockData[] blocks, int maxPacketSize)
        {
            var messages = new List<UpdateMapResponse>();
            var positionBuffer = new List<Vector3Int>(maxPacketSize);
            var blockBuffer = new List<BlockData>(maxPacketSize);
            for (var i = 0; i < blocks.Length; i++)
            {
                positionBuffer.Add(positions[i]);
                blockBuffer.Add(blocks[i]);
                if ((positionBuffer.Count + 1) * sizeof(int) * 4 >= maxPacketSize)
                {
                    messages.Add(new UpdateMapResponse(positionBuffer.ToArray(), blockBuffer.ToArray()));
                    positionBuffer.Clear();
                    blockBuffer.Clear();
                }
            }

            if (positionBuffer.Count > 0)
            {
                messages.Add(new UpdateMapResponse(positionBuffer.ToArray(), blockBuffer.ToArray()));
            }

            return messages.ToArray();
        }

        public override IEnumerator SendMessages(UpdateMapResponse[] messages, NetworkConnectionToClient destination,
            float delayInSeconds)
        {
            for (var i = 0; i < messages.Length; i++)
            {
                NetworkServer.SendToAll(messages[i]);
                yield return new WaitForSeconds(delayInSeconds);
            }
        }
    }
}
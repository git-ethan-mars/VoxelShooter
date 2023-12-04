using System.Collections;
using System.Collections.Generic;
using Data;
using Mirror;
using Networking.Messages.Responses;
using UnityEngine;

namespace Networking
{
    public class BlockSplitter
    {
        public UpdateMapResponse[] SplitArraysIntoMessages(Vector3Int[] positions, BlockData[] blocks, int maxPacketSize)
        {
            var messages = new List<UpdateMapResponse>();
            var positionBuffer = new List<Vector3Int>();
            var blockBuffer = new List<BlockData>();
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
                messages.Add(new UpdateMapResponse(positionBuffer.ToArray(), blockBuffer.ToArray()));

            return messages.ToArray();
        }
        
        public FallBlockResponse[] SplitArraysIntoMessages(Vector3Int[] positions, Color32[] blocks, int maxPacketSize)
        {
            var messages = new List<FallBlockResponse>();
            var positionBuffer = new List<Vector3Int>();
            var blockBuffer = new List<Color32>();
            var size = sizeof(int) * 8;
            for (var i = 0; i < blocks.Length; i++)
            {
                positionBuffer.Add(positions[i]);
                blockBuffer.Add(blocks[i]);
                if ((positionBuffer.Count + 1) * size >= maxPacketSize)
                {
                    messages.Add(new FallBlockResponse(positionBuffer.ToArray(), blockBuffer.ToArray()));
                    positionBuffer.Clear();
                    blockBuffer.Clear();
                }
            }

            if (positionBuffer.Count > 0)
                messages.Add(new FallBlockResponse(positionBuffer.ToArray(), blockBuffer.ToArray()));

            return messages.ToArray();
        }

        public IEnumerator SendMessages(UpdateMapResponse[] messages, float delayInSeconds)
        {
            for (var i = 0; i < messages.Length; i++)
            {
                NetworkServer.SendToAll(messages[i]);
                yield return new WaitForSeconds(delayInSeconds);
            }
        }

        public IEnumerator SendMessages(FallBlockResponse[] messages, float delayInSeconds)
        {
            for (var i = 0; i < messages.Length; i++)
            {
                NetworkServer.SendToAll(messages[i]);
                yield return new WaitForSeconds(delayInSeconds);
            }
        }
    }
}
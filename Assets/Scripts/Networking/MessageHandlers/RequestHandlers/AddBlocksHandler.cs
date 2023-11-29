using System;
using System.Collections.Generic;
using Data;
using Mirror;
using Networking.Messages.Requests;
using Networking.Messages.Responses;
using UnityEngine;

namespace Networking.MessageHandlers.RequestHandlers
{
    public class AddBlocksHandler : RequestHandler<AddBlocksRequest>
    {
        private readonly IServer _server;

        public AddBlocksHandler(IServer server)
        {
            _server = server;
        }

        protected override void OnRequestReceived(NetworkConnectionToClient connection, AddBlocksRequest request)
        {
            var result = _server.Data.TryGetPlayerData(connection, out var playerData);
            if (!result || !playerData.IsAlive) return;
            var blockAmount = playerData.CountByItem[playerData.Items[playerData.SelectedSlotIndex]];
            var validPositions = new List<Vector3Int>();
            var validBlockData = new List<BlockData>();
            var blocksUsed = Math.Min(blockAmount, request.GlobalPositions.Length);
            for (var i = 0; i < blocksUsed; i++)
            {
                foreach (var otherConnection in _server.Data.ClientConnections)
                {
                    result = _server.Data.TryGetPlayerData(otherConnection, out var otherPlayer);
                    if (!result || !otherPlayer.IsAlive) continue;
                    var playerPosition = otherConnection.identity.gameObject.transform.position;
                    var blockPosition = request.GlobalPositions[i];
                    if (playerPosition.x > blockPosition.x
                        && playerPosition.x < blockPosition.x + 1
                        && playerPosition.z > blockPosition.z
                        && playerPosition.z < blockPosition.z + 1
                        && playerPosition.y > blockPosition.y - 2
                        && playerPosition.y < blockPosition.y + 2)
                        return;
                }

                if (!_server.MapProvider.IsDestructiblePosition(request.GlobalPositions[i]))
                {
                    return;
                }

                var currentBlock = _server.MapProvider.GetBlockByGlobalPosition(request.GlobalPositions[i]);
                if (currentBlock.Equals(request.Blocks[i]))
                {
                    return;
                }

                validPositions.Add(request.GlobalPositions[i]);
                validBlockData.Add(request.Blocks[i]);
            }

            playerData.CountByItem[playerData.Items[playerData.SelectedSlotIndex]] = blockAmount - blocksUsed;
            connection.Send(new ItemUseResponse(playerData.SelectedSlotIndex,
                blockAmount - blocksUsed));
            _server.MapUpdater.SetBlocksByGlobalPositions(validPositions, validBlockData);
        }
    }
}
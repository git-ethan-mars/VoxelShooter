using System;
using System.Collections.Generic;
using Data;
using Mirror;
using Networking.Messages.Requests;
using Networking.Messages.Responses;

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
            if (!result || !playerData.IsAlive || playerData.SelectedItem is not BlockItem)
            {
                return;
            }

            var blockAmount = playerData.CountByItem[playerData.SelectedItem];
            var validBlocks = new List<BlockDataWithPosition>();
            var blocksUsed = Math.Min(blockAmount, request.Blocks.Length);
            for (var i = 0; i < blocksUsed; i++)
            {
                var blockPosition = request.Blocks[i].Position;
                foreach (var otherConnection in _server.Data.ClientConnections)
                {
                    result = _server.Data.TryGetPlayerData(otherConnection, out var otherPlayer);
                    if (!result || !otherPlayer.IsAlive)
                    {
                        continue;
                    }

                    var playerPosition = otherConnection.identity.gameObject.transform.position;
                    if (playerPosition.x > blockPosition.x
                        && playerPosition.x < blockPosition.x + 1
                        && playerPosition.z > blockPosition.z
                        && playerPosition.z < blockPosition.z + 1
                        && playerPosition.y > blockPosition.y - 2
                        && playerPosition.y < blockPosition.y + 2)
                        return;
                }

                if (!_server.MapProvider.IsDestructiblePosition(blockPosition))
                {
                    return;
                }

                var currentBlock = _server.MapProvider.GetBlockByGlobalPosition(blockPosition);
                if (currentBlock.Equals(request.Blocks[i].BlockData))
                {
                    return;
                }

                validBlocks.Add(request.Blocks[i]);
            }

            playerData.CountByItem[playerData.SelectedItem] = blockAmount - blocksUsed;
            connection.Send(new ItemUseResponse(playerData.SelectedSlotIndex,
                blockAmount - blocksUsed));
            _server.BlockHealthSystem.InitializeBlocks(validBlocks);
        }
    }
}
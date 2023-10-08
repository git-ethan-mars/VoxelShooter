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
            for (var i = 0; i < request.GlobalPositions.Length; i++)
            {
                var index = _server.MapDestructionAlgorithm.GetVertexIndex(request.GlobalPositions[i]);
                _server.MapProvider.MapData._solidBlocks.Add(index);
                _server.MapProvider.MapData._blocksPlacedByPlayer.Add(index);
                _server.MapProvider.MapData._blockColors[index] = request.Blocks[i].Color;
            }

            var result = _server.ServerData.TryGetPlayerData(connection, out var playerData);
            if (!result || !playerData.IsAlive) return;
            var blockAmount = playerData.ItemCountById[request.ItemId];
            var validPositions = new List<Vector3Int>();
            var validBlockData = new List<BlockData>();
            var blocksUsed = Math.Min(blockAmount, request.GlobalPositions.Length);
            for (var i = 0; i < blocksUsed; i++)
            {
                foreach (var otherConnection in _server.ServerData.GetConnections())
                {
                    result = _server.ServerData.TryGetPlayerData(otherConnection, out var otherPlayer);
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

                if (!_server.MapProvider.IsValidPosition(request.GlobalPositions[i])) return;
                var currentBlock = _server.MapProvider.GetBlockByGlobalPosition(request.GlobalPositions[i]);
                if (currentBlock.Equals(request.Blocks[i])) return;
                _server.MapUpdater.SetBlockByGlobalPosition(request.GlobalPositions[i], request.Blocks[i]);
                validPositions.Add(request.GlobalPositions[i]);
                validBlockData.Add(request.Blocks[i]);
            }

            playerData.ItemCountById[request.ItemId] = blockAmount - blocksUsed;
            NetworkServer.SendToAll(new UpdateMapResponse(validPositions.ToArray(), validBlockData.ToArray()));
            connection.Send(new ItemUseResponse(request.ItemId, blockAmount - blocksUsed));
        }
    }
}
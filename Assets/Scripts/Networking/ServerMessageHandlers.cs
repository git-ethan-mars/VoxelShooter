using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using Infrastructure;
using Infrastructure.Factory;
using Mirror;
using Networking.Messages;
using Steamworks;
using UnityEngine;

namespace Networking
{
    public class ServerMessageHandlers
    {
        private readonly IEntityFactory _entityFactory;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly ServerData _serverData;
        private readonly IPlayerFactory _playerFactory;
        private readonly bool _isLocalBuild;

        public ServerMessageHandlers(IEntityFactory entityFactory, ICoroutineRunner coroutineRunner,
            ServerData serverData, IPlayerFactory playerFactory, bool isLocalBuild)
        {
            _serverData = serverData;
            _entityFactory = entityFactory;
            _coroutineRunner = coroutineRunner;
            _playerFactory = playerFactory;
            _isLocalBuild = isLocalBuild;
        }

        public void RegisterHandlers()
        {
            NetworkServer.RegisterHandler<ChangeClassRequest>(OnChangeClass);
            NetworkServer.RegisterHandler<TntSpawnRequest>(OnTntSpawn);
            NetworkServer.RegisterHandler<AddBlocksRequest>(OnAddBlocks);
            NetworkServer.RegisterHandler<RemoveBlocksRequest>(OnRemoveBlocks);
        }

        public void RemoveHandlers()
        {
            NetworkServer.UnregisterHandler<ChangeClassRequest>();
            NetworkServer.UnregisterHandler<TntSpawnRequest>();
            NetworkServer.UnregisterHandler<AddBlocksRequest>();
            NetworkServer.UnregisterHandler<RemoveBlocksRequest>();
        }

        private void OnChangeClass(NetworkConnectionToClient conn, ChangeClassRequest message)
        {
            var nick = _isLocalBuild ? "" : SteamFriends.GetPersonaName();
            var player = _playerFactory.CreatePlayer(message.GameClass, nick);
            NetworkServer.AddPlayerForConnection(conn, player);
            _serverData.AddPlayer(conn, message.GameClass, nick);
        }

        private void OnAddBlocks(NetworkConnectionToClient connection, AddBlocksRequest message)
        {
            var blockAmount = _serverData.GetItemCount(connection, message.ItemId);
            var validPositionList = new List<Vector3Int>();
            var validBlockDataList = new List<BlockData>();
            var blocksUsed = Math.Min(blockAmount, message.GlobalPositions.Length);
            for (var i = 0; i < blocksUsed; i++)
            {
                if (!_serverData.Map.IsValidPosition(message.GlobalPositions[i])) continue;
                var currentBlock = _serverData.Map.GetBlockByGlobalPosition(message.GlobalPositions[i]);
                if (currentBlock.Equals(message.Blocks[i])) continue;
                validPositionList.Add(message.GlobalPositions[i]);
                validBlockDataList.Add(message.Blocks[i]);
            }
            _serverData.SetItemCount(connection, message.ItemId, blockAmount - blocksUsed);
            NetworkServer.SendToAll(new UpdateMapMessage(validPositionList.ToArray(), validBlockDataList.ToArray()));
            connection.Send(new ItemUseResult(message.ItemId, blockAmount - blocksUsed));
        }

        private void OnRemoveBlocks(NetworkConnectionToClient connection, RemoveBlocksRequest message)
        {
            var validPositionList = new List<Vector3Int>();
            for (var i = 0; i < message.GlobalPositions.Length; i++)
            {
                if (!_serverData.Map.IsValidPosition(message.GlobalPositions[i])) continue;
                validPositionList.Add(message.GlobalPositions[i]);
            }

            NetworkServer.SendToAll(new UpdateMapMessage(validPositionList.ToArray(),
                new BlockData[validPositionList.Count]));
        }

        private void OnTntSpawn(NetworkConnectionToClient connection, TntSpawnRequest message)
        {
            var tntCount = _serverData.GetItemCount(connection, message.ItemId);
            if (tntCount <= 0)
                return;
            var tnt = _entityFactory.CreateTnt(message.Position, message.Rotation);
            _coroutineRunner.StartCoroutine(ExplodeWithDelay(Vector3Int.FloorToInt(message.ExplosionCenter), tnt,
                message.DelayInSecond,
                message.Radius));
            _serverData.SetItemCount(connection, message.ItemId, tntCount - 1);
            connection.Send(new ItemUseResult(message.ItemId, tntCount - 1));
        }

        private IEnumerator ExplodeWithDelay(Vector3Int explosionCenter, GameObject tnt, float delayInSeconds,
            int radius)
        {
            yield return new WaitForSeconds(delayInSeconds);
            var validPositions = new List<Vector3Int>();
            for (var x = -radius; x <= radius; x++)
            {
                for (var y = -radius; y <= radius; y++)
                {
                    for (var z = -radius; z <= radius; z++)
                    {
                        var blockPosition = new Vector3Int(explosionCenter.x,
                            explosionCenter.y, explosionCenter.z) + new Vector3Int(x, y, z);
                        if (_serverData.Map.IsValidPosition(blockPosition) && Vector3Int.Distance(blockPosition, explosionCenter) <= radius)
                            validPositions.Add(blockPosition);
                    }
                }
            }

            foreach (var position in validPositions)
            {
                _serverData.Map.SetBlockByGlobalPosition(position, new BlockData());
            }

            NetworkServer.SendToAll(new UpdateMapMessage(validPositions.ToArray(),
                new BlockData[validPositions.Count]));
            NetworkServer.Destroy(tnt);
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using Infrastructure;
using Infrastructure.Factory;
using Mirror;
using Networking.Messages;
using UnityEngine;

namespace Networking
{
    public class ServerMessageHandlers
    {
        private readonly IEntityFactory _entityFactory;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly ServerData _serverData;

        public ServerMessageHandlers(IEntityFactory entityFactory, ICoroutineRunner coroutineRunner,
            ServerData serverData)
        {
            _serverData = serverData;
            _entityFactory = entityFactory;
            _coroutineRunner = coroutineRunner;
        }

        public void RegisterHandlers()
        {
            NetworkServer.RegisterHandler<ChangeClassRequest>(OnChangeClass);
            NetworkServer.RegisterHandler<TntSpawnRequest>(OnTntSpawn);
            NetworkServer.RegisterHandler<AddBlocksRequest>(OnAddBlocks);
            NetworkServer.RegisterHandler<RemoveBlocksRequest>(OnRemoveBlocks);
            NetworkServer.RegisterHandler<ChangeSlotRequest>(OnChangeSlot);
        }

        public void RemoveHandlers()
        {
            NetworkServer.UnregisterHandler<ChangeClassRequest>();
            NetworkServer.UnregisterHandler<TntSpawnRequest>();
            NetworkServer.UnregisterHandler<AddBlocksRequest>();
            NetworkServer.UnregisterHandler<RemoveBlocksRequest>();
            NetworkServer.UnregisterHandler<ChangeSlotRequest>();
        }

        private void OnChangeClass(NetworkConnectionToClient connection, ChangeClassRequest message)
        {
            var playerData = _serverData.GetPlayerData(connection);
            if (playerData is null)
            {
                _serverData.AddPlayer(connection, message.GameClass, message.SteamID, message.Nickname);
            }
            else
            {
                _serverData.ChangeClass(connection, message.GameClass);
            }
        }

        private void OnAddBlocks(NetworkConnectionToClient connection, AddBlocksRequest message)
        {
            if (connection.identity == null) return;
            var blockAmount = _serverData.GetItemCount(connection, message.ItemId);
            var validPositionList = new List<Vector3Int>();
            var validBlockDataList = new List<BlockData>();
            var blocksUsed = Math.Min(blockAmount, message.GlobalPositions.Length);

            for (var i = 0; i < blocksUsed; i++)
            {
                foreach (var player in _serverData.DataByConnection.Keys)
                {
                    if (player.identity != null)
                    {
                        var playerPosition = player.identity.gameObject.transform.position;
                        var blockPosition = message.GlobalPositions[i];
                        if (playerPosition.x > blockPosition.x
                            && playerPosition.x < blockPosition.x + 1
                            && playerPosition.z > blockPosition.z
                            && playerPosition.z < blockPosition.z + 1
                            && playerPosition.y > blockPosition.y - 2
                            && playerPosition.y < blockPosition.y + 2)
                            return;
                    }
                }

                if (!_serverData.Map.IsValidPosition(message.GlobalPositions[i])) return;
                var currentBlock = _serverData.Map.GetBlockByGlobalPosition(message.GlobalPositions[i]);
                if (currentBlock.Equals(message.Blocks[i])) return;
                validPositionList.Add(message.GlobalPositions[i]);
                validBlockDataList.Add(message.Blocks[i]);
            }

            _serverData.SetItemCount(connection, message.ItemId, blockAmount - blocksUsed);
            NetworkServer.SendToAll(new UpdateMapMessage(validPositionList.ToArray(), validBlockDataList.ToArray()));
            connection.Send(new ItemUseResult(message.ItemId, blockAmount - blocksUsed));
        }

        private void OnRemoveBlocks(NetworkConnectionToClient connection, RemoveBlocksRequest message)
        {
            if (connection.identity == null) return;
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
            if (connection.identity == null) return;
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
            if (!tnt) yield break;
            var explodedTnt = new List<GameObject>();
            ExplodeImmediately(explosionCenter, tnt, radius, explodedTnt);
        }

        private void ExplodeImmediately(Vector3Int explosionCenter, GameObject tnt, int radius,
            List<GameObject> explodedTnt)
        {
            var validPositions = new List<Vector3Int>();
            for (var x = -radius; x <= radius; x++)
            {
                for (var y = -radius; y <= radius; y++)
                {
                    for (var z = -radius; z <= radius; z++)
                    {
                        var blockPosition = new Vector3Int(explosionCenter.x,
                            explosionCenter.y, explosionCenter.z) + new Vector3Int(x, y, z);
                        if (_serverData.Map.IsValidPosition(blockPosition) &&
                            Vector3Int.Distance(blockPosition, explosionCenter) <= radius)
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
            explodedTnt.Add(tnt);

            Collider[] hitColliders = Physics.OverlapSphere(explosionCenter, radius);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("TNT") && explodedTnt.All(x => x.gameObject != hitCollider.gameObject))
                {
                    ExplodeImmediately(Vector3Int.FloorToInt(hitCollider.gameObject.transform.position),
                        hitCollider.gameObject, radius, explodedTnt);
                }
            }
        }

        private void OnChangeSlot(NetworkConnectionToClient connection, ChangeSlotRequest message)
        {
            if (connection.identity == null) return;
            connection.identity.GetComponent<PlayerLogic.Inventory>().currentSlotId = message.Index;
            connection.Send(new ChangeSlotResult(message.Index));
        }
    }
}
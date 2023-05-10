using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            NetworkServer.RegisterHandler<ChangeSlotRequest>(OnChangeSlot);
            NetworkServer.RegisterHandler<KillerCameraRequest>(OnKillerCameraRequest);
            NetworkServer.RegisterHandler<NextPlayerCameraRequest>(OnNextCameraRequest);
        }

        public void RemoveHandlers()
        {
            NetworkServer.UnregisterHandler<ChangeClassRequest>();
            NetworkServer.UnregisterHandler<TntSpawnRequest>();
            NetworkServer.UnregisterHandler<AddBlocksRequest>();
            NetworkServer.UnregisterHandler<RemoveBlocksRequest>();
            NetworkServer.UnregisterHandler<ChangeSlotRequest>();
            NetworkServer.UnregisterHandler<KillerCameraRequest>();
            NetworkServer.UnregisterHandler<NextPlayerCameraRequest>();
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
            connection.identity.gameObject.GetComponent<PlayerLogic.Inventory>().currentSlotId = message.Index;
            connection.Send(new ChangeSlotResult(message.Index));
        }

        private void OnNextCameraRequest(NetworkConnectionToClient connection, NextPlayerCameraRequest message)
        {
            var alivePlayers = _serverData.DataByConnection.Where(kvp => kvp.Value.GameClass != GameClass.None)
                .ToList();
            if (alivePlayers.Count == 0)
            {
                var mapWidth = _serverData.Map.MapData.Width;
                var mapHeight = _serverData.Map.MapData.Height;
                var mapDepth = _serverData.Map.MapData.Depth;
                connection.Send(new SpectatorTargetResult(null, new Vector3(mapWidth / 2, mapHeight / 2, mapDepth / 2 )));
                return;
            }
            var index = 0;
            for (; index < alivePlayers.Count; index++)
            {
                if (alivePlayers[index].Key.connectionId == message.ConnectionId)
                {
                    connection.Send(
                        new SpectatorTargetResult(alivePlayers[(index + 1) % alivePlayers.Count].Key.identity, Vector3.zero));
                    return;
                }
            }

            connection.Send(new SpectatorTargetResult(alivePlayers[0].Key.identity, Vector3.zero));
        }

        private void OnKillerCameraRequest(NetworkConnectionToClient connection, KillerCameraRequest message)
        {
            for (var i = _serverData.Kills.Count - 1; i >= 0; i--)
            {
                var killData = _serverData.Kills[i];
                if (killData.Victim != connection) continue;
                if (_serverData.GetPlayerData(killData.Killer).GameClass == GameClass.None)
                {
                    OnNextCameraRequest(connection, new NextPlayerCameraRequest(0));
                    return;
                }

                connection.Send(new SpectatorTargetResult(killData.Killer.identity, Vector3.zero));
                return;
            }

            OnNextCameraRequest(connection, new NextPlayerCameraRequest(0));
        }
    }
}
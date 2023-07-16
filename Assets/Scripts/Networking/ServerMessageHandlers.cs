using System;
using System.Collections.Generic;
using Data;
using Infrastructure;
using Infrastructure.Factory;
using Infrastructure.Services.StaticData;
using Mirror;
using Networking.Messages;
using UnityEngine;
using Explosions;

namespace Networking
{
    public partial class ServerMessageHandlers
    {
        private readonly Server _server;
        private readonly IEntityFactory _entityFactory;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly IStaticDataService _staticData;
        private readonly IParticleFactory _particleFactory;
        private readonly ExplosionBehaviour _singleExplosionBehaviour;
        private readonly ExplosionBehaviour _chainExplosionBehaviour;

        public ServerMessageHandlers(IEntityFactory entityFactory, ICoroutineRunner coroutineRunner,
            Server server, IStaticDataService staticData, IParticleFactory particleFactory)
        {
            _server = server;
            _entityFactory = entityFactory;
            _coroutineRunner = coroutineRunner;
            _staticData = staticData;
            _particleFactory = particleFactory;
            _singleExplosionBehaviour =
                new SingleExplosionBehaviour(_server.MapUpdater, particleFactory,
                    new SphereExplosionArea(_server.MapProvider));
            _chainExplosionBehaviour =
                new ChainExplosionBehaviour(_server.MapUpdater, particleFactory,
                    new SphereExplosionArea(_server.MapProvider));
        }

        public void RegisterHandlers()
        {
            NetworkServer.RegisterHandler<ChangeClassRequest>(OnChangeClass);
            NetworkServer.RegisterHandler<TntSpawnRequest>(OnTntSpawn);
            NetworkServer.RegisterHandler<GrenadeSpawnRequest>(OnGrenadeSpawn);
            NetworkServer.RegisterHandler<RocketLauncherSpawnRequest>(OnRocketLauncherSpawn);
            NetworkServer.RegisterHandler<AddBlocksRequest>(OnAddBlocks);
            NetworkServer.RegisterHandler<ChangeSlotRequest>(OnChangeSlot);
        }

        public void RemoveHandlers()
        {
            NetworkServer.UnregisterHandler<ChangeClassRequest>();
            NetworkServer.UnregisterHandler<TntSpawnRequest>();
            NetworkServer.UnregisterHandler<GrenadeSpawnRequest>();
            NetworkServer.UnregisterHandler<RocketLauncherSpawnRequest>();
            NetworkServer.UnregisterHandler<AddBlocksRequest>();
            NetworkServer.UnregisterHandler<ChangeSlotRequest>();
        }

        private void OnChangeClass(NetworkConnectionToClient connection, ChangeClassRequest message)
        {
            var result = _server.ServerData.TryGetPlayerData(connection, out _);
            if (!result)
                _server.AddPlayer(connection, message.GameClass, message.SteamID, message.Nickname);
            else
                _server.ChangeClass(connection, message.GameClass);
        }

        private void OnAddBlocks(NetworkConnectionToClient connection, AddBlocksRequest message)
        {
            var result = _server.ServerData.TryGetPlayerData(connection, out var playerData);
            if (!result || !playerData.IsAlive) return;
            var blockAmount = playerData.ItemCountById[message.ItemId];
            var validPositions = new List<Vector3Int>();
            var validBlockData = new List<BlockData>();
            var blocksUsed = Math.Min(blockAmount, message.GlobalPositions.Length);
            for (var i = 0; i < blocksUsed; i++)
            {
                foreach (var otherConnection in _server.ServerData.GetConnections())
                {
                    result = _server.ServerData.TryGetPlayerData(otherConnection, out var otherPlayer);
                    if (!result || !otherPlayer.IsAlive) continue;
                    var playerPosition = otherConnection.identity.gameObject.transform.position;
                    var blockPosition = message.GlobalPositions[i];
                    if (playerPosition.x > blockPosition.x
                        && playerPosition.x < blockPosition.x + 1
                        && playerPosition.z > blockPosition.z
                        && playerPosition.z < blockPosition.z + 1
                        && playerPosition.y > blockPosition.y - 2
                        && playerPosition.y < blockPosition.y + 2)
                        return;
                }

                if (!_server.MapProvider.IsValidPosition(message.GlobalPositions[i])) return;
                var currentBlock = _server.MapProvider.GetBlockByGlobalPosition(message.GlobalPositions[i]);
                if (currentBlock.Equals(message.Blocks[i])) return;
                _server.MapUpdater.SetBlockByGlobalPosition(message.GlobalPositions[i], message.Blocks[i]);
                validPositions.Add(message.GlobalPositions[i]);
                validBlockData.Add(message.Blocks[i]);
            }

            playerData.ItemCountById[message.ItemId] = blockAmount - blocksUsed;
            NetworkServer.SendToAll(new UpdateMapMessage(validPositions.ToArray(), validBlockData.ToArray()));
            connection.Send(new ItemUseResult(message.ItemId, blockAmount - blocksUsed));
        }

        private void OnChangeSlot(NetworkConnectionToClient connection, ChangeSlotRequest message)
        {
            var result = _server.ServerData.TryGetPlayerData(connection, out var playerData);
            if (!result || !playerData.IsAlive) return;
            connection.identity.GetComponent<PlayerLogic.Inventory>().currentSlotId = message.Index;
            connection.Send(new ChangeSlotResult(message.Index));
        }
    }
}
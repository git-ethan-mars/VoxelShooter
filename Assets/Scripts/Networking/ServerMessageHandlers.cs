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
        }

        public void RemoveHandlers()
        {
            NetworkServer.UnregisterHandler<ChangeClassRequest>();
            NetworkServer.UnregisterHandler<TntSpawnRequest>();
        }

        private void OnChangeClass(NetworkConnectionToClient conn, ChangeClassRequest message)
        {
            var nick = _isLocalBuild ? "" : SteamFriends.GetPersonaName();
            var player = _playerFactory.CreatePlayer(message.GameClass, nick);
            NetworkServer.AddPlayerForConnection(conn, player);
            _serverData.AddPlayer(conn, message.GameClass, nick);
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
                        if (Vector3Int.Distance(blockPosition, explosionCenter) <= radius)
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
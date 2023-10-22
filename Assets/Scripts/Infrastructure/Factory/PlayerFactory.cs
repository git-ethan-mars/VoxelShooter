using System.Collections.Generic;
using CameraLogic;
using Data;
using Infrastructure.AssetManagement;
using Mirror;
using Networking;
using Networking.Messages.Responses;
using PlayerLogic.States;
using UnityEngine;

namespace Infrastructure.Factory
{
    public class PlayerFactory : IPlayerFactory
    {
        private readonly List<SpawnPointData> _spawnPoints;
        private readonly IAssetProvider _assets;
        private readonly IServer _server;
        private int _spawnPointIndex;
        private const string PlayerPath = "Prefabs/Player";
        private const string SpectatorPlayerPath = "Prefabs/Spectator player";


        public PlayerFactory(IServer server,
            IAssetProvider assets)
        {
            _server = server;
            _spawnPoints = _server.MapProvider.SceneData.SpawnPoints;
            _assets = assets;
        }


        public void CreatePlayer(NetworkConnectionToClient connection)
        {
            GameObject player;
            if (_spawnPoints.Count == 0)
            {
                player = CreatePlayerWithoutSpawnPoint();
            }
            else
            {
                player = CreatePlayerWithSpawnPoint(_spawnPoints[_spawnPointIndex].ToVectorWithOffset(),
                    Quaternion.identity);
                _spawnPointIndex = (_spawnPointIndex + 1) % _spawnPoints.Count;
            }

            var playerData = _server.ServerData.GetPlayerData(connection);
            playerData.PlayerStateMachine.Enter<LifeState>();
            NetworkServer.AddPlayerForConnection(connection, player);
            connection.Send(new PlayerConfigureResponse(playerData.Characteristic.placeDistance,
                playerData.Characteristic.speed, playerData.Characteristic.jumpMultiplier, playerData.ItemIds));
        }

        public void RespawnPlayer(NetworkConnectionToClient connection)
        {
            var result = _server.ServerData.TryGetPlayerData(connection, out var playerData);
            if (!result) return;
            GameObject player;
            if (_spawnPoints.Count == 0)
            {
                player = CreatePlayerWithoutSpawnPoint();
            }
            else
            {
                player = CreatePlayerWithSpawnPoint(_spawnPoints[_spawnPointIndex].ToVectorWithOffset(),
                    Quaternion.identity);
                _spawnPointIndex = (_spawnPointIndex + 1) % _spawnPoints.Count;
            }

            playerData.PlayerStateMachine.Enter<LifeState>();
            ReplacePlayer(connection, player);
            connection.Send(new PlayerConfigureResponse(playerData.Characteristic.placeDistance,
                playerData.Characteristic.speed, playerData.Characteristic.jumpMultiplier, playerData.ItemIds));
        }

        public void CreateSpectatorPlayer(NetworkConnectionToClient connection)
        {
            var spectator = _assets.Instantiate(SpectatorPlayerPath);
            spectator.GetComponent<Spectator>().Construct(_server);
            ReplacePlayer(connection, spectator);
        }

        private GameObject CreatePlayerWithSpawnPoint(Vector3 position,
            Quaternion rotation) =>
            _assets.Instantiate(PlayerPath, position, rotation);

        private GameObject CreatePlayerWithoutSpawnPoint() => _assets.Instantiate(PlayerPath);

        private void ReplacePlayer(NetworkConnectionToClient connection, GameObject newPlayer)
        {
            if (connection.identity == null)
            {
                return;
            }

            var oldPlayer = connection.identity.gameObject;
            NetworkServer.ReplacePlayerForConnection(connection, newPlayer, true);
            Object.Destroy(oldPlayer, 0.1f);
        }
    }
}
using System;
using System.Collections.Generic;
using Data;
using Infrastructure.Factory;
using Infrastructure.Services;
using Mirror;
using UnityEngine;

namespace Networking
{
    public class CustomNetworkManager : NetworkManager
    {
        public event Action ConnectionHappened;
        private List<SpawnPoint> _spawnPoints;
        private int _spawnPointIndex;
        private IMapProvider _mapProvider;
        private IGameFactory _gameFactory;


        public void Construct(IMapProvider mapProvider, IGameFactory gameFactory)
        {
            _mapProvider = mapProvider;
            _gameFactory = gameFactory;
            _spawnPoints = _mapProvider.Map.MapData.SpawnPoints;
        }

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            base.OnServerConnect(conn);
            ConnectionHappened?.Invoke();
        }

        public override void OnClientConnect()
        {
            base.OnClientConnect();
            ConnectionHappened?.Invoke();
        }
        /*
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            GameObject player;
            if (_spawnPoints.Count == 0)
            {
                player = _gameFactory.CreatePlayer();
            }
            else
            {
                player = _gameFactory.CreatePlayer(_spawnPoints[_spawnPointIndex].ToUnityVector(),
                    Quaternion.identity);
                _spawnPointIndex = (_spawnPointIndex + 1) % _spawnPoints.Count;
            }

            NetworkServer.AddPlayerForConnection(conn, player);
        }
        */
    }
}
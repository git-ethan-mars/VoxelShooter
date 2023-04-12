using System;
using System.Collections.Generic;
using Data;
using Infrastructure.Factory;
using Infrastructure.Services;
using MapLogic;
using Mirror;
using Networking.Synchronization;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Networking
{
    public class CustomNetworkManager : NetworkManager
    {
        public event Action ConnectionHappened;
        private List<SpawnPoint> _spawnPoints;
        private int _spawnPointIndex;
        private IMapProvider _mapProvider;
        private IGameFactory _gameFactory;
        private IStaticDataService _staticData;
        private ServerData _serverData;

        public void Construct(IMapProvider mapProvider, IGameFactory gameFactory, IStaticDataService staticDataService)
        {
            _mapProvider = mapProvider;
            _gameFactory = gameFactory;
            _staticData = staticDataService;
            _spawnPoints = _mapProvider.Map.MapData.SpawnPoints;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            _serverData = new ServerData(_staticData);
            NetworkServer.RegisterHandler<CharacterMessage>(OnCreateCharacter);
        }
        

        public override void OnClientConnect()
        {
            base.OnClientConnect();
            ConnectionHappened?.Invoke();
            CharacterMessage characterMessage = new CharacterMessage() {GameClass = (GameClass)Random.Range(0,4), NickName = ""};
            NetworkClient.Send(characterMessage);
        }

        private void OnCreateCharacter(NetworkConnectionToClient conn, CharacterMessage message)
        {
            GameClass gameClass = message.GameClass;
            GameObject player;
            if (_spawnPoints.Count == 0)
            {
                player = _gameFactory.CreatePlayer(gameClass);
            }
            else
            {
                player = _gameFactory.CreatePlayer(gameClass,_spawnPoints[_spawnPointIndex].ToUnityVector(),
                    Quaternion.identity);
                _spawnPointIndex = (_spawnPointIndex + 1) % _spawnPoints.Count;
            }
            player.GetComponent<WeaponSynchronization>().Construct(_gameFactory,_serverData);
            _serverData.AddPlayer(conn, message.GameClass, message.NickName);
            NetworkServer.AddPlayerForConnection(conn, player);
        }
        

        public override void OnStopServer()
        {
            MapWriter.SaveMap("test.rch", _mapProvider.Map);
        }
    }
}
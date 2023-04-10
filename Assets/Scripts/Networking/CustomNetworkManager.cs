using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Infrastructure.Factory;
using Infrastructure.Services;
using MapLogic;
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
        private IStaticDataService _staticData;


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
            NetworkServer.RegisterHandler<CharacterMessage>(OnCreateCharacter);
        }
        

        public override void OnClientConnect()
        {
            base.OnClientConnect();
            ConnectionHappened?.Invoke();
            NetworkClient.RegisterHandler<InventoryMessage>(OnInventory);
            CharacterMessage characterMessage = new CharacterMessage() {GameClass = GameClass.Builder, NickName = ""};
            NetworkClient.Send(characterMessage);
        }

        private void OnCreateCharacter(NetworkConnectionToClient conn, CharacterMessage message)
        {
            GameClass gameClass = message.GameClass;
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
            var inventory = _staticData.GetInventory(gameClass).Select(item => item.id).ToArray();
            InventoryMessage inventoryMessage = new InventoryMessage() {Inventory = inventory};
            conn.Send(inventoryMessage);
        }


        private void OnInventory(InventoryMessage message)
        {
            NetworkClient.localPlayer.gameObject.GetComponent<Player.Inventory>().SetInventory(message.Inventory);
        }

        public override void OnStopServer()
        {
            MapWriter.SaveMap("test.rch", _mapProvider.Map);
        }
    }
}
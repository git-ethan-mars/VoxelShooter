using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Data;
using Infrastructure.Factory;
using Infrastructure.Services;
using MapLogic;
using Mirror;
using PlayerLogic;

namespace Networking
{
    public class CustomNetworkManager : NetworkManager
    {
        public ServerData ServerData { get; private set; }
        public IEntityFactory EntityFactory { get; set; }
        public event Action<Map> MapLoaded;
        public event Action<Map> MapDownloaded;
        private int _spawnPointIndex;
        private List<byte> _byteChunks;
        private Map _map;
        private IStaticDataService _staticData;

        public void Construct(IStaticDataService staticData)
        {
            _staticData = staticData;
            _byteChunks = new List<byte>();
        }

        public override void OnStartServer()
        {
            ServerData = new ServerData(_staticData);
            NetworkServer.RegisterHandler<CharacterMessage>(OnChooseClass);
            _map = MapReader.ReadFromFile("lastsav.vxl");
            MapLoaded?.Invoke(_map);
        }

        public override void OnStartClient()
        {
            NetworkClient.RegisterHandler<MapMessage>(OnMapLoad);
            NetworkClient.RegisterHandler<HealthMessage>(OnHealthChange);
        }


        public override void OnServerReady(NetworkConnectionToClient conn)
        {
            base.OnServerReady(conn);
            if (NetworkClient.connection.connectionId == conn.connectionId) return;
            MapMessage[] mapMessages = SplitMap(100000);
            StartCoroutine(SendMap(conn, mapMessages));
        }

        private IEnumerator SendMap(NetworkConnectionToClient conn, MapMessage[] messages)
        {
            for (var i = 0; i < messages.Length; i++)
            {
                conn.Send(messages[i]);
                yield return null;
            }
        }


        public override void OnStopServer()
        {
            MapWriter.SaveMap("lastsav.rch", _map);
        }

        public void ChangeClass(GameClass gameClass)
        {
            CharacterMessage characterMessage = new CharacterMessage()
                {GameClass = gameClass, NickName = ""};
            NetworkClient.Send(characterMessage);
        }


        private void OnMapLoad(MapMessage mapMessage)
        {
            _byteChunks.AddRange(mapMessage.ByteChunk);
            if (!mapMessage.IsFinalChunk) return;
            _map = MapReader.ReadFromStream(new MemoryStream(_byteChunks.ToArray()));
            MapDownloaded?.Invoke(_map);
        }

        private void OnChooseClass(NetworkConnectionToClient conn, CharacterMessage message)
        {
            var player = EntityFactory.CreatePlayer(conn, message);
            NetworkServer.AddPlayerForConnection(conn, player);
            ServerData.AddPlayer(conn, message.GameClass, message.NickName);
        }


        private void OnHealthChange(HealthMessage healthMessage)
        {
            NetworkClient.localPlayer.gameObject.GetComponent<HealthSystem>()
                .UpdateHealth(healthMessage.CurrentHealth, healthMessage.MaxHealth);
        }


        private MapMessage[] SplitMap(int packageSize)
        {
            var memoryStream = new MemoryStream();
            MapWriter.WriteMap(_map, memoryStream);
            var bytes = memoryStream.ToArray();
            var messages = new List<MapMessage>();

            for (var i = 0; i < bytes.Length; i += packageSize)
            {
                if (bytes.Length <= i + packageSize)
                {
                    messages.Add(new MapMessage(bytes[i..bytes.Length], true));
                }
                else
                {
                    messages.Add(new MapMessage(bytes[i..(i + packageSize)], false));
                }
            }

            return messages.ToArray();
        }
    }
}